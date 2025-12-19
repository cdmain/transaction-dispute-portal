using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;

namespace AuthService.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly AuthServiceImpl _service;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<AuthServiceImpl>> _loggerMock;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthServiceImpl>>();
        
        // Setup configuration
        _configMock.Setup(c => c["Jwt:Secret"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("DisputePortal");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("DisputePortalUsers");
        _configMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");
        
        _service = new AuthServiceImpl(_context, _configMock.Object, _loggerMock.Object);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Test",
            LastName = "User",
            CustomerId = "CUST001",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task RegisterAsync_CreatesNewUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "NewPass123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.User.Email.Should().Be("newuser@example.com");
        result.User.FirstName.Should().Be("New");
        result.User.CustomerId.Should().StartWith("CUST");
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenEmailExists()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ReturnsAuthResponse_WhenCredentialsValid()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.User.Email.Should().Be("test@example.com");
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenEmailNotFound()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Test123!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UpdatesLastLoginAt()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        // Act
        await _service.LoginAsync(request);

        // Assert
        var user = await _context.Users.FirstAsync(u => u.Email == "test@example.com");
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsNewTokens_WhenValid()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };
        var loginResult = await _service.LoginAsync(loginRequest);
        var refreshToken = loginResult!.RefreshToken;

        // Act
        var result = await _service.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsNull_WhenTokenInvalid()
    {
        // Act
        var result = await _service.RefreshTokenAsync("invalid-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeTokenAsync_RevokesToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };
        var loginResult = await _service.LoginAsync(loginRequest);
        var refreshToken = loginResult!.RefreshToken;

        // Act
        var result = await _service.RevokeTokenAsync(refreshToken);

        // Assert
        result.Should().BeTrue();
        
        // Verify token can't be used
        var refreshResult = await _service.RefreshTokenAsync(refreshToken);
        refreshResult.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.CustomerId.Should().Be("CUST001");
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsTrue_WhenTokenValid()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };
        var loginResult = await _service.LoginAsync(loginRequest);
        var token = loginResult!.Token;

        // Act
        var result = await _service.ValidateTokenAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFalse_WhenTokenInvalid()
    {
        // Act
        var result = await _service.ValidateTokenAsync("invalid-token");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
