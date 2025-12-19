namespace DisputeService.Models;

public class Dispute
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }
    public DisputeCategory Category { get; set; }
    public decimal DisputedAmount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? TransactionReference { get; set; }
    public string? MerchantName { get; set; }
}

public enum DisputeStatus
{
    Pending,
    UnderReview,
    AwaitingDocuments,
    Resolved,
    Rejected,
    Cancelled
}

public enum DisputeCategory
{
    UnauthorizedTransaction,
    DuplicateCharge,
    IncorrectAmount,
    ServiceNotReceived,
    ProductNotReceived,
    QualityIssue,
    RefundNotReceived,
    FraudSuspected,
    Other
}
