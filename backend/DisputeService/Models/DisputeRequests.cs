using System.ComponentModel.DataAnnotations;

namespace DisputeService.Models;

public class CreateDisputeRequest
{
    [Required]
    public Guid TransactionId { get; set; }

    [Required]
    [StringLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DisputeCategory Category { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal DisputedAmount { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = "ZAR";

    [StringLength(100)]
    public string? TransactionReference { get; set; }

    [StringLength(200)]
    public string? MerchantName { get; set; }
}

public class UpdateDisputeStatusRequest
{
    [Required]
    public DisputeStatus Status { get; set; }

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }
}

public class DisputeQueryParameters
{
    public string? CustomerId { get; set; }
    public Guid? TransactionId { get; set; }
    public DisputeStatus? Status { get; set; }
    public DisputeCategory? Category { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
