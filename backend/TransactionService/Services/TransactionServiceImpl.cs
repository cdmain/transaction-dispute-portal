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

    public async Task<IEnumerable<Transaction>> SeedTransactionsForCustomerAsync(string customerId)
    {
        // Check if customer already has transactions
        var existingCount = await _context.Transactions.CountAsync(t => t.CustomerId == customerId);
        if (existingCount > 0)
        {
            _logger.LogInformation("Customer {CustomerId} already has {Count} transactions", customerId, existingCount);
            return await GetTransactionsByCustomerIdAsync(customerId);
        }

        var merchants = new[]
        {
            ("Amazon", "Shopping", "Retail"),
            ("Woolworths", "Groceries", "Supermarket"),
            ("Pick n Pay", "Groceries", "Supermarket"),
            ("Takealot", "Shopping", "Online Retail"),
            ("Netflix", "Entertainment", "Streaming"),
            ("Spotify", "Entertainment", "Streaming"),
            ("Uber", "Transportation", "Ride Share"),
            ("Mr D Food", "Food & Dining", "Delivery"),
            ("Engen", "Gas & Fuel", "Petrol Station"),
            ("Dischem", "Health", "Pharmacy"),
            ("Game", "Electronics", "Retail"),
            ("Checkers", "Groceries", "Supermarket"),
            ("Builders Warehouse", "Home", "Hardware"),
            ("Vida e Caffe", "Food & Dining", "Coffee Shop"),
            ("Spur", "Food & Dining", "Restaurant")
        };

        var random = new Random();
        var transactions = new List<Transaction>();
        var now = DateTime.UtcNow;

        // Generate 25 transactions over the last 90 days
        for (int i = 0; i < 25; i++)
        {
            var daysAgo = random.Next(0, 90);
            var merchant = merchants[random.Next(merchants.Length)];
            var amount = Math.Round((decimal)(random.NextDouble() * 2000 + 50), 2);
            var isCredit = random.NextDouble() > 0.9;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Description = $"Transaction at {merchant.Item1}",
                Amount = amount,
                Currency = "ZAR",
                Category = merchant.Item2,
                MerchantName = merchant.Item1,
                MerchantCategory = merchant.Item3,
                Type = isCredit ? TransactionType.Credit : TransactionType.Debit,
                Status = random.NextDouble() > 0.05 ? TransactionStatus.Completed : TransactionStatus.Pending,
                TransactionDate = now.AddDays(-daysAgo).AddHours(random.Next(8, 22)).AddMinutes(random.Next(0, 60)),
                CreatedAt = now.AddDays(-daysAgo),
                Reference = $"TXN{now.Year}{(now.Month):D2}{random.Next(100000, 999999)}",
                CardLastFourDigits = "4242",
                IsDisputed = false
            };

            transactions.Add(transaction);
        }

        // Sort by date descending
        transactions = transactions.OrderByDescending(t => t.TransactionDate).ToList();

        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} transactions for customer {CustomerId}", transactions.Count, customerId);

        return transactions;
    }
}
