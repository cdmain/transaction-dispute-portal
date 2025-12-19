namespace TransactionService.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Category { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string MerchantCategory { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Reference { get; set; }
    public string? CardLastFourDigits { get; set; }
    public bool IsDisputed { get; set; }
}

public enum TransactionType
{
    Debit,
    Credit
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Reversed
}
