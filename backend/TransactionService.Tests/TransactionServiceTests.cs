using Microsoft.EntityFrameworkCore;
using TransactionService.Data;
using TransactionService.Models;
using TransactionService.Services;
using FluentAssertions;

namespace TransactionService.Tests;

public class TransactionServiceTests : IDisposable
{
    private readonly TransactionDbContext _context;
    private readonly TransactionServiceImpl _service;

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TransactionDbContext(options);
        _service = new TransactionServiceImpl(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Reference = "TXN001",
                CustomerId = "CUST001",
                MerchantName = "Amazon",
                Amount = 100.00m,
                Currency = "ZAR",
                Category = "Shopping",
                Description = "Online purchase",
                TransactionDate = DateTime.UtcNow.AddDays(-5),
                Type = TransactionType.Debit,
                Status = TransactionStatus.Completed,
                IsDisputed = false
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Reference = "TXN002",
                CustomerId = "CUST001",
                MerchantName = "Uber",
                Amount = 50.00m,
                Currency = "ZAR",
                Category = "Transport",
                Description = "Ride payment",
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                Type = TransactionType.Debit,
                Status = TransactionStatus.Completed,
                IsDisputed = true
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Reference = "TXN003",
                CustomerId = "CUST002",
                MerchantName = "Woolworths",
                Amount = 200.00m,
                Currency = "ZAR",
                Category = "Groceries",
                Description = "Weekly groceries",
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                Type = TransactionType.Debit,
                Status = TransactionStatus.Completed,
                IsDisputed = false
            }
        };

        _context.Transactions.AddRange(transactions);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetTransactionsAsync_ReturnsAllTransactions_WhenNoFilters()
    {
        // Arrange
        var parameters = new TransactionQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetTransactionsAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetTransactionsAsync_FiltersByCustomerId()
    {
        // Arrange
        var parameters = new TransactionQueryParameters 
        { 
            CustomerId = "CUST001",
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetTransactionsAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(t => t.CustomerId.Should().Be("CUST001"));
    }

    [Fact]
    public async Task GetTransactionsAsync_FiltersByCategory()
    {
        // Arrange
        var parameters = new TransactionQueryParameters 
        { 
            Category = "Shopping",
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetTransactionsAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Category.Should().Be("Shopping");
    }

    [Fact]
    public async Task GetTransactionsAsync_FiltersByDateRange()
    {
        // Arrange
        var parameters = new TransactionQueryParameters 
        { 
            StartDate = DateTime.UtcNow.AddDays(-4),
            EndDate = DateTime.UtcNow,
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetTransactionsAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTransactionsAsync_FiltersByDisputedStatus()
    {
        // Arrange
        var parameters = new TransactionQueryParameters 
        { 
            IsDisputed = true,
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetTransactionsAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().IsDisputed.Should().BeTrue();
    }

    [Fact]
    public async Task GetTransactionsAsync_SearchesByMerchantName()
    {
        // Arrange
        var parameters = new TransactionQueryParameters 
        { 
            SearchTerm = "Amazon",
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _service.GetTransactionsAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().MerchantName.Should().Be("Amazon");
    }

    [Fact]
    public async Task GetTransactionsAsync_PaginatesCorrectly()
    {
        // Arrange
        var parameters = new TransactionQueryParameters 
        { 
            Page = 1, 
            PageSize = 2 
        };

        // Act
        var result = await _service.GetTransactionsAsync(parameters);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_ReturnsTransaction_WhenExists()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetTransactionByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Reference.Should().Be("TXN001");
    }

    [Fact]
    public async Task GetTransactionByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _service.GetTransactionByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTransactionsByCustomerIdAsync_ReturnsCustomerTransactions()
    {
        // Act
        var result = await _service.GetTransactionsByCustomerIdAsync("CUST001");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.CustomerId.Should().Be("CUST001"));
    }

    [Fact]
    public async Task MarkAsDisputedAsync_UpdatesTransaction()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.MarkAsDisputedAsync(id);

        // Assert
        result.Should().BeTrue();
        var transaction = await _service.GetTransactionByIdAsync(id);
        transaction!.IsDisputed.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsDisputedAsync_ReturnsFalse_WhenNotExists()
    {
        // Act
        var result = await _service.MarkAsDisputedAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnmarkAsDisputedAsync_UpdatesTransaction()
    {
        // Arrange
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        var result = await _service.UnmarkAsDisputedAsync(id);

        // Assert
        result.Should().BeTrue();
        var transaction = await _service.GetTransactionByIdAsync(id);
        transaction!.IsDisputed.Should().BeFalse();
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsUniqueCategories()
    {
        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(new[] { "Shopping", "Transport", "Groceries" });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
