namespace Identity.Tests.Application;

using BuildingBlocks.Application.Exceptions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Infrastructure.Services;
using Moq;
using Xunit;

public class IdentityServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();

        _jwtTokenServiceMock.Setup(x => x.CreateTokens(It.IsAny<ApplicationUser>()))
            .Returns(new TokenResponse("mock-access-token", DateTime.UtcNow.AddMinutes(15), "mock-refresh-token", DateTime.UtcNow.AddDays(30)));

        _service = new IdentityService(_userRepositoryMock.Object, _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_CreatesUserAndReturnsTokens()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.PhoneExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.RegisterAsync(
            "Test User",
            "9876543210",
            "test@example.com",
            "P@ssword123",
            "WebBrowser"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test User", result.User.FullName);
        Assert.Equal("test@example.com", result.User.Email);
        Assert.Equal("mock-access-token", result.Tokens.AccessToken);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicatePhone_ThrowsConflictException()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.PhoneExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync("Test User", "9876543210", "test@example.com", "P@ssword123", null));
    }

    [Fact]
    public async Task RegisterAsync_InvalidPassword_ThrowsValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.RegisterAsync("Test User", "9876543210", "test@example.com", "short", null));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("P@ssword123");
        var user = new ApplicationUser(Guid.NewGuid(), "Jane User", "9876543210", passwordHash, "jane@example.com");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync("jane@example.com", "P@ssword123", "iPhone");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane User", result.User.FullName);
        Assert.Equal("mock-access-token", result.Tokens.AccessToken);
        _userRepositoryMock.Verify(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("P@ssword123");
        var user = new ApplicationUser(Guid.NewGuid(), "Jane User", "9876543210", passwordHash, "jane@example.com");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync("jane@example.com", "WrongPassword", null));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("P@ssword123");
        var user = new ApplicationUser(Guid.NewGuid(), "Jane User", "9876543210", passwordHash, "jane@example.com");
        user.Block();

        _userRepositoryMock.Setup(x => x.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync("jane@example.com", "P@ssword123", null));
    }
}

