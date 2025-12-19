using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using DisputeService.Models;
using DisputeService.Services;

namespace DisputeService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<DisputesController> _logger;
    
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    public DisputesController(
        IDisputeService disputeService, 
        IDistributedCache cache,
        ILogger<DisputesController> logger)
    {
        _disputeService = disputeService;
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
    /// Get all disputes for the authenticated user with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Dispute>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Dispute>>> GetDisputes([FromQuery] DisputeQueryParameters parameters)
    {
        // Override customer ID with authenticated user's ID
        var customerId = GetCustomerIdFromToken();
        parameters.CustomerId = customerId;
        
        _logger.LogInformation("Getting disputes for customer {CustomerId} with parameters: {@Parameters}", 
            customerId, parameters);
        
        var result = await _disputeService.GetDisputesAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get a dispute by ID (must belong to authenticated user)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Dispute), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Dispute>> GetDispute(Guid id)
    {
        var customerId = GetCustomerIdFromToken();
        var dispute = await _disputeService.GetDisputeByIdAsync(id);
        
        if (dispute == null)
        {
            return NotFound(new { message = $"Dispute with ID {id} not found" });
        }
        
        // Ensure dispute belongs to authenticated user
        if (dispute.CustomerId != customerId)
        {
            _logger.LogWarning("User {CustomerId} attempted to access dispute {DisputeId} belonging to another user", 
                customerId, id);
            return Forbid();
        }
        
        return Ok(dispute);
    }

    /// <summary>
    /// Get all disputes for a customer (requires matching customer ID)
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<Dispute>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<Dispute>>> GetDisputesByCustomer(string customerId)
    {
        var authenticatedCustomerId = GetCustomerIdFromToken();
        
        // Users can only access their own disputes
        if (customerId != authenticatedCustomerId)
        {
            return Forbid();
        }
        
        var disputes = await _disputeService.GetDisputesByCustomerIdAsync(customerId);
        return Ok(disputes);
    }

    /// <summary>
    /// Get all disputes for a transaction (must belong to authenticated user)
    /// </summary>
    [HttpGet("transaction/{transactionId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<Dispute>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Dispute>>> GetDisputesByTransaction(Guid transactionId)
    {
        var customerId = GetCustomerIdFromToken();
        var disputes = await _disputeService.GetDisputesByTransactionIdAsync(transactionId);
        
        // Filter to only return disputes belonging to authenticated user
        var userDisputes = disputes.Where(d => d.CustomerId == customerId);
        return Ok(userDisputes);
    }

    /// <summary>
    /// Create a new dispute
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Dispute), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Dispute>> CreateDispute([FromBody] CreateDisputeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Override customer ID with authenticated user's ID
        var customerId = GetCustomerIdFromToken();
        request.CustomerId = customerId;

        try
        {
            var dispute = await _disputeService.CreateDisputeAsync(request);
            
            // Invalidate cache
            await InvalidateCustomerCacheAsync(customerId);
            
            return CreatedAtAction(nameof(GetDispute), new { id = dispute.Id }, dispute);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update dispute status (must belong to authenticated user)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(Dispute), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Dispute>> UpdateDisputeStatus(Guid id, [FromBody] UpdateDisputeStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var customerId = GetCustomerIdFromToken();
        var existingDispute = await _disputeService.GetDisputeByIdAsync(id);
        
        if (existingDispute == null)
        {
            return NotFound(new { message = $"Dispute with ID {id} not found" });
        }
        
        if (existingDispute.CustomerId != customerId)
        {
            return Forbid();
        }

        var dispute = await _disputeService.UpdateDisputeStatusAsync(id, request);
        
        // Invalidate cache
        await InvalidateCustomerCacheAsync(customerId);

        return Ok(dispute);
    }

    /// <summary>
    /// Cancel a dispute (must belong to authenticated user)
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelDispute(Guid id)
    {
        var customerId = GetCustomerIdFromToken();
        var existingDispute = await _disputeService.GetDisputeByIdAsync(id);
        
        if (existingDispute == null)
        {
            return NotFound(new { message = $"Dispute with ID {id} not found" });
        }
        
        if (existingDispute.CustomerId != customerId)
        {
            return Forbid();
        }
        
        try
        {
            var result = await _disputeService.CancelDisputeAsync(id);
            
            // Invalidate cache
            await InvalidateCustomerCacheAsync(customerId);
            
            return Ok(new { message = "Dispute cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get dispute statistics for authenticated user
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(DisputeStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<DisputeStatistics>> GetStatistics()
    {
        var customerId = GetCustomerIdFromToken();
        
        // Try to get from cache
        var cacheKey = $"disputes:statistics:{customerId}";
        var cachedResult = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedResult))
        {
            return Ok(JsonSerializer.Deserialize<DisputeStatistics>(cachedResult));
        }
        
        var statistics = await _disputeService.GetStatisticsAsync(customerId);
        
        // Cache the result
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(statistics), CacheOptions);
        
        return Ok(statistics);
    }
    
    private async Task InvalidateCustomerCacheAsync(string customerId)
    {
        _logger.LogInformation("Cache invalidated for customer {CustomerId}", customerId);
        // Clear statistics cache
        await _cache.RemoveAsync($"disputes:statistics:{customerId}");
    }
}
