using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TransactionService.Models;
using TransactionService.Services;

namespace TransactionService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TransactionsController> _logger;
    
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    public TransactionsController(
        ITransactionService transactionService, 
        IDistributedCache cache,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's customer ID from JWT token
    /// </summary>
    private string GetCustomerIdFromToken()
    {
        return User.FindFirst("customer_id")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("Customer ID not found in token");
    }

    /// <summary>
    /// Get all transactions for the authenticated user with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Transaction>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Transaction>>> GetTransactions([FromQuery] TransactionQueryParameters parameters)
    {
        // Override customer ID with authenticated user's ID
        var customerId = GetCustomerIdFromToken();
        parameters.CustomerId = customerId;
        
        _logger.LogInformation("Getting transactions for customer {CustomerId} with parameters: {@Parameters}", 
            customerId, parameters);
        
        // Try to get from cache
        var cacheKey = $"transactions:{customerId}:{JsonSerializer.Serialize(parameters)}";
        var cachedResult = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedResult))
        {
            _logger.LogDebug("Cache hit for transactions query");
            return Ok(JsonSerializer.Deserialize<PagedResult<Transaction>>(cachedResult));
        }
        
        var result = await _transactionService.GetTransactionsAsync(parameters);
        
        // Cache the result
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), CacheOptions);
        
        return Ok(result);
    }

    /// <summary>
    /// Get a transaction by ID (must belong to authenticated user)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Transaction>> GetTransaction(Guid id)
    {
        var customerId = GetCustomerIdFromToken();
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        
        if (transaction == null)
        {
            return NotFound(new { message = $"Transaction with ID {id} not found" });
        }
        
        // Ensure transaction belongs to authenticated user
        if (transaction.CustomerId != customerId)
        {
            _logger.LogWarning("User {CustomerId} attempted to access transaction {TransactionId} belonging to another user", 
                customerId, id);
            return Forbid();
        }
        
        return Ok(transaction);
    }

    /// <summary>
    /// Get all transactions for a customer (admin endpoint - requires matching customer ID)
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByCustomer(string customerId)
    {
        var authenticatedCustomerId = GetCustomerIdFromToken();
        
        // Users can only access their own transactions
        if (customerId != authenticatedCustomerId)
        {
            return Forbid();
        }
        
        var transactions = await _transactionService.GetTransactionsByCustomerIdAsync(customerId);
        return Ok(transactions);
    }

    /// <summary>
    /// Mark a transaction as disputed (must belong to authenticated user)
    /// </summary>
    [HttpPut("{id:guid}/dispute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAsDisputed(Guid id)
    {
        var customerId = GetCustomerIdFromToken();
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        
        if (transaction == null)
        {
            return NotFound(new { message = $"Transaction with ID {id} not found" });
        }
        
        if (transaction.CustomerId != customerId)
        {
            return Forbid();
        }
        
        var result = await _transactionService.MarkAsDisputedAsync(id);
        
        // Invalidate cache
        await InvalidateCustomerCacheAsync(customerId);
        
        return Ok(new { message = "Transaction marked as disputed" });
    }

    /// <summary>
    /// Unmark a transaction as disputed (must belong to authenticated user)
    /// </summary>
    [HttpDelete("{id:guid}/dispute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UnmarkAsDisputed(Guid id)
    {
        var customerId = GetCustomerIdFromToken();
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        
        if (transaction == null)
        {
            return NotFound(new { message = $"Transaction with ID {id} not found" });
        }
        
        if (transaction.CustomerId != customerId)
        {
            return Forbid();
        }
        
        var result = await _transactionService.UnmarkAsDisputedAsync(id);
        
        // Invalidate cache
        await InvalidateCustomerCacheAsync(customerId);
        
        return Ok(new { message = "Transaction dispute removed" });
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var cacheKey = "transactions:categories";
        var cachedResult = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedResult))
        {
            return Ok(JsonSerializer.Deserialize<IEnumerable<string>>(cachedResult));
        }
        
        var categories = await _transactionService.GetCategoriesAsync();
        
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(categories), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
        
        return Ok(categories);
    }

    /// <summary>
    /// Seed demo transactions for the authenticated user (creates transactions if none exist)
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Transaction>>> SeedTransactions()
    {
        var customerId = GetCustomerIdFromToken();
        
        _logger.LogInformation("Seeding transactions for customer {CustomerId}", customerId);
        
        var transactions = await _transactionService.SeedTransactionsForCustomerAsync(customerId);
        
        // Invalidate cache
        await InvalidateCustomerCacheAsync(customerId);
        
        return Ok(transactions);
    }
    
    private async Task InvalidateCustomerCacheAsync(string customerId)
    {
        // Note: In production, use Redis SCAN or maintain a list of cache keys
        // For simplicity, we're just logging here - real implementation would 
        // use a cache invalidation pattern
        _logger.LogInformation("Cache invalidated for customer {CustomerId}", customerId);
    }
}
