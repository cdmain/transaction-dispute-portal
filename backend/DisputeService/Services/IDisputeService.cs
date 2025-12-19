using DisputeService.Models;

namespace DisputeService.Services;

public interface IDisputeService
{
    Task<PagedResult<Dispute>> GetDisputesAsync(DisputeQueryParameters parameters);
    Task<Dispute?> GetDisputeByIdAsync(Guid id);
    Task<IEnumerable<Dispute>> GetDisputesByCustomerIdAsync(string customerId);
    Task<IEnumerable<Dispute>> GetDisputesByTransactionIdAsync(Guid transactionId);
    Task<Dispute> CreateDisputeAsync(CreateDisputeRequest request);
    Task<Dispute?> UpdateDisputeStatusAsync(Guid id, UpdateDisputeStatusRequest request);
    Task<bool> CancelDisputeAsync(Guid id);
    Task<DisputeStatistics> GetStatisticsAsync(string? customerId = null);
}

public class DisputeStatistics
{
    public int TotalDisputes { get; set; }
    public int PendingDisputes { get; set; }
    public int UnderReviewDisputes { get; set; }
    public int ResolvedDisputes { get; set; }
    public int RejectedDisputes { get; set; }
    public decimal TotalDisputedAmount { get; set; }
    public decimal ResolvedAmount { get; set; }
}
