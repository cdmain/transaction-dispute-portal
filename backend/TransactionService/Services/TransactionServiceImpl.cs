using Microsoft.EntityFrameworkCore;
using TransactionService.Data;
using TransactionService.Models;

namespace TransactionService.Services;

public class TransactionServiceImpl : ITransactionService
{
    private readonly TransactionDbContext _context;
    private readonly ILogger<TransactionServiceImpl> _logger;

    public TransactionServiceImpl(TransactionDbContext context, ILogger<TransactionServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<Transaction>> GetTransactionsAsync(TransactionQueryParameters parameters)
    {
        var query = _context.Transactions.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(parameters.CustomerId))
        {
            query = query.Where(t => t.CustomerId == parameters.CustomerId);
        }

        if (parameters.FromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= parameters.FromDate.Value);
        }

        if (parameters.ToDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= parameters.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(parameters.Category))
        {
            query = query.Where(t => t.Category == parameters.Category);
        }

        if (parameters.Type.HasValue)
        {
            query = query.Where(t => t.Type == parameters.Type.Value);
        }

        if (parameters.MinAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= parameters.MinAmount.Value);
        }

        if (parameters.MaxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= parameters.MaxAmount.Value);
        }

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            query = query.Where(t => 
                t.Description.ToLower().Contains(searchTerm) ||
                t.MerchantName.ToLower().Contains(searchTerm) ||
                t.Reference!.ToLower().Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<Transaction>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByCustomerIdAsync(string customerId)
    {
        return await _context.Transactions
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<bool> MarkAsDisputedAsync(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return false;
        }

        transaction.IsDisputed = true;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction {TransactionId} marked as disputed", id);
        return true;
    }

    public async Task<bool> UnmarkAsDisputedAsync(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return false;
        }

        transaction.IsDisputed = false;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction {TransactionId} unmarked as disputed", id);
        return true;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Transactions
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
