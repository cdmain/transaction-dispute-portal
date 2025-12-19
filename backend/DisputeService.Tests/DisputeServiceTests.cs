using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DisputeService.Data;
using DisputeService.Models;
using DisputeService.Services;
using FluentAssertions;

namespace DisputeService.Tests;

public class DisputeServiceTests : IDisposable
{
    private readonly DisputeDbContext _context;
    private readonly DisputeServiceImpl _service;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<DisputeServiceImpl>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;

    public DisputeServiceTests()
    {
        var options = new DbContextOptionsBuilder<DisputeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisputeDbContext(options);
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<DisputeServiceImpl>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        
        _configMock.Setup(c => c["TransactionService:BaseUrl"]).Returns("http://localhost:5001");
        
        var httpClient = new HttpClient();
        _service = new DisputeServiceImpl(_context, _loggerMock.Object, httpClient, _configMock.Object);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var disputes = new List<Dispute>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                TransactionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CustomerId = "CUST001",
                Reason = "Unauthorized charge",
                Description = "I did not authorize this transaction",
                DisputedAmount = 100.00m,
                Category = DisputeCategory.UnauthorizedTransaction,
                Status = DisputeStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                TransactionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                CustomerId = "CUST001",
                Reason = "Double charge",
                Description = "I was charged twice",
                DisputedAmount = 50.00m,
                Category = DisputeCategory.DuplicateCharge,
                Status = DisputeStatus.UnderReview,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                TransactionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                CustomerId = "CUST002",
                Reason = "Product not received",
                Description = "Never received the product",
                DisputedAmount = 200.00m,
                Category = DisputeCategory.ProductNotReceived,
                Status = DisputeStatus.Resolved,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ResolvedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                TransactionId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                CustomerId = "CUST001",
                Reason = "Wrong amount",
                Description = "Charged wrong amount",
                DisputedAmount = 75.00m,
                Category = DisputeCategory.IncorrectAmount,
                Status = DisputeStatus.Rejected,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ResolvedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Disputes.AddRange(disputes);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetDisputesAsync_ReturnsAllDisputes_WhenNoFilters()
    {
        // Arrange
        var parameters = new DisputeQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetDisputesAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task GetDisputesAsync_FiltersByCustomerId()
    {
        // Arrange
        var parameters = new DisputeQueryParameters 
        { 
            CustomerId = "CUST001",
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetDisputesAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().AllSatisfy(d => d.CustomerId.Should().Be("CUST001"));
    }

    [Fact]
    public async Task GetDisputesAsync_FiltersByStatus()
    {
        // Arrange
        var parameters = new DisputeQueryParameters 
        { 
            Status = DisputeStatus.Pending,
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetDisputesAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be(DisputeStatus.Pending);
    }

    [Fact]
    public async Task GetDisputesAsync_FiltersByCategory()
    {
        // Arrange
        var parameters = new DisputeQueryParameters 
        { 
            Category = DisputeCategory.DuplicateCharge,
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetDisputesAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Category.Should().Be(DisputeCategory.DuplicateCharge);
    }

    [Fact]
    public async Task GetDisputesAsync_PaginatesCorrectly()
    {
        // Arrange
        var parameters = new DisputeQueryParameters 
        { 
            Page = 1, 
            PageSize = 2 
        };

        // Act
        var result = await _service.GetDisputesAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetDisputeByIdAsync_ReturnsDispute_WhenExists()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetDisputeByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Reason.Should().Be("Unauthorized charge");
    }

    [Fact]
    public async Task GetDisputeByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetDisputeByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDisputesByCustomerIdAsync_ReturnsCustomerDisputes()
    {
        // Act
        var result = await _service.GetDisputesByCustomerIdAsync("CUST001");

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(d => d.CustomerId.Should().Be("CUST001"));
    }

    [Fact]
    public async Task GetDisputesByTransactionIdAsync_ReturnsDisputes()
    {
        // Arrange
        var transactionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var result = await _service.GetDisputesByTransactionIdAsync(transactionId);

        // Assert
        result.Should().HaveCount(1);
        result.First().TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public async Task UpdateDisputeStatusAsync_UpdatesStatus()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new UpdateDisputeStatusRequest
        {
            Status = DisputeStatus.UnderReview
        };

        // Act
        var result = await _service.UpdateDisputeStatusAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(DisputeStatus.UnderReview);
    }

    [Fact]
    public async Task UpdateDisputeStatusAsync_SetsResolvedAt_WhenResolved()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new UpdateDisputeStatusRequest
        {
            Status = DisputeStatus.Resolved,
            ResolutionNotes = "Refund issued"
        };

        // Act
        var result = await _service.UpdateDisputeStatusAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(DisputeStatus.Resolved);
        result.ResolvedAt.Should().NotBeNull();
        result.ResolutionNotes.Should().Be("Refund issued");
    }

    [Fact]
    public async Task CancelDisputeAsync_CancelsDispute()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.CancelDisputeAsync(id);

        // Assert
        result.Should().BeTrue();
        var dispute = await _service.GetDisputeByIdAsync(id);
        dispute!.Status.Should().Be(DisputeStatus.Cancelled);
    }

    [Fact]
    public async Task CancelDisputeAsync_ThrowsException_WhenAlreadyResolved()
    {
        // Arrange
        var id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CancelDisputeAsync(id));
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStats_ForAllDisputes()
    {
        // Act
        var result = await _service.GetStatisticsAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.TotalDisputes.Should().Be(4);
        result.PendingDisputes.Should().Be(1);
        result.UnderReviewDisputes.Should().Be(1);
        result.ResolvedDisputes.Should().Be(1);
        result.RejectedDisputes.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStats_ForCustomer()
    {
        // Act
        var result = await _service.GetStatisticsAsync("CUST001");

        // Assert
        result.Should().NotBeNull();
        result.TotalDisputes.Should().Be(3);
        result.PendingDisputes.Should().Be(1);
        result.UnderReviewDisputes.Should().Be(1);
        result.RejectedDisputes.Should().Be(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
