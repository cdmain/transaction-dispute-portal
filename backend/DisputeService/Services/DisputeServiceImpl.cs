using Microsoft.EntityFrameworkCore;
using DisputeService.Data;
using DisputeService.Models;

namespace DisputeService.Services;

public class DisputeServiceImpl : IDisputeService
{
    private readonly DisputeDbContext _context;
    private readonly ILogger<DisputeServiceImpl> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public DisputeServiceImpl(
        DisputeDbContext context, 
        ILogger<DisputeServiceImpl> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<PagedResult<Dispute>> GetDisputesAsync(DisputeQueryParameters parameters)
    {
        var query = _context.Disputes.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.CustomerId))
        {
            query = query.Where(d => d.CustomerId == parameters.CustomerId);
        }

        if (parameters.TransactionId.HasValue)
        {
            query = query.Where(d => d.TransactionId == parameters.TransactionId.Value);
        }

        if (parameters.Status.HasValue)
        {
            query = query.Where(d => d.Status == parameters.Status.Value);
        }

        if (parameters.Category.HasValue)
        {
            query = query.Where(d => d.Category == parameters.Category.Value);
        }

        if (parameters.FromDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= parameters.FromDate.Value);
        }

        if (parameters.ToDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= parameters.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<Dispute>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<Dispute?> GetDisputeByIdAsync(Guid id)
    {
        return await _context.Disputes.FindAsync(id);
    }

    public async Task<IEnumerable<Dispute>> GetDisputesByCustomerIdAsync(string customerId)
    {
        return await _context.Disputes
            .Where(d => d.CustomerId == customerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Dispute>> GetDisputesByTransactionIdAsync(Guid transactionId)
    {
        return await _context.Disputes
            .Where(d => d.TransactionId == transactionId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Dispute> CreateDisputeAsync(CreateDisputeRequest request)
    {
        // Check if there's already an active dispute for this transaction
        var existingDispute = await _context.Disputes
            .FirstOrDefaultAsync(d => 
                d.TransactionId == request.TransactionId && 
                d.Status != DisputeStatus.Resolved && 
                d.Status != DisputeStatus.Rejected &&
                d.Status != DisputeStatus.Cancelled);

        if (existingDispute != null)
        {
            throw new InvalidOperationException("An active dispute already exists for this transaction");
        }

        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            TransactionId = request.TransactionId,
            CustomerId = request.CustomerId,
            Reason = request.Reason,
            Description = request.Description,
            Category = request.Category,
            DisputedAmount = request.DisputedAmount,
            Currency = request.Currency,
            TransactionReference = request.TransactionReference,
            MerchantName = request.MerchantName,
            Status = DisputeStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync();

        // Notify Transaction Service to mark transaction as disputed
        await NotifyTransactionServiceAsync(request.TransactionId, true);

        _logger.LogInformation("Created dispute {DisputeId} for transaction {TransactionId}", 
            dispute.Id, dispute.TransactionId);

        return dispute;
    }

    public async Task<Dispute?> UpdateDisputeStatusAsync(Guid id, UpdateDisputeStatusRequest request)
    {
        var dispute = await _context.Disputes.FindAsync(id);
        if (dispute == null)
        {
            return null;
        }

        dispute.Status = request.Status;
        dispute.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.ResolutionNotes))
        {
            dispute.ResolutionNotes = request.ResolutionNotes;
        }

        if (request.Status == DisputeStatus.Resolved || request.Status == DisputeStatus.Rejected)
        {
            dispute.ResolvedAt = DateTime.UtcNow;
            
            // If resolved or rejected, unmark the transaction as disputed
            if (request.Status == DisputeStatus.Rejected)
            {
                await NotifyTransactionServiceAsync(dispute.TransactionId, false);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated dispute {DisputeId} status to {Status}", id, request.Status);

        return dispute;
    }

    public async Task<bool> CancelDisputeAsync(Guid id)
    {
        var dispute = await _context.Disputes.FindAsync(id);
        if (dispute == null)
        {
            return false;
        }

        if (dispute.Status == DisputeStatus.Resolved || dispute.Status == DisputeStatus.Rejected)
        {
            throw new InvalidOperationException("Cannot cancel a resolved or rejected dispute");
        }

        dispute.Status = DisputeStatus.Cancelled;
        dispute.UpdatedAt = DateTime.UtcNow;
        dispute.ResolvedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Unmark transaction as disputed
        await NotifyTransactionServiceAsync(dispute.TransactionId, false);

        _logger.LogInformation("Cancelled dispute {DisputeId}", id);

        return true;
    }

    public async Task<DisputeStatistics> GetStatisticsAsync(string? customerId = null)
    {
        var query = _context.Disputes.AsQueryable();

        if (!string.IsNullOrEmpty(customerId))
        {
            query = query.Where(d => d.CustomerId == customerId);
        }

        var disputes = await query.ToListAsync();

        return new DisputeStatistics
        {
            TotalDisputes = disputes.Count,
            PendingDisputes = disputes.Count(d => d.Status == DisputeStatus.Pending),
            UnderReviewDisputes = disputes.Count(d => d.Status == DisputeStatus.UnderReview),
            ResolvedDisputes = disputes.Count(d => d.Status == DisputeStatus.Resolved),
            RejectedDisputes = disputes.Count(d => d.Status == DisputeStatus.Rejected),
            TotalDisputedAmount = disputes.Sum(d => d.DisputedAmount),
            ResolvedAmount = disputes
                .Where(d => d.Status == DisputeStatus.Resolved)
                .Sum(d => d.DisputedAmount)
        };
    }

    private async Task NotifyTransactionServiceAsync(Guid transactionId, bool isDisputed)
    {
        try
        {
            var baseUrl = _configuration["TransactionService:BaseUrl"] ?? "http://localhost:5001";
            var endpoint = isDisputed 
                ? $"{baseUrl}/api/transactions/{transactionId}/dispute"
                : $"{baseUrl}/api/transactions/{transactionId}/dispute";

            if (isDisputed)
            {
                await _httpClient.PutAsync(endpoint, null);
            }
            else
            {
                await _httpClient.DeleteAsync(endpoint);
            }

            _logger.LogInformation("Notified Transaction Service: Transaction {TransactionId} disputed = {IsDisputed}", 
                transactionId, isDisputed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify Transaction Service about dispute status change");
            // Don't throw - this is a non-critical operation
        }
    }
}
