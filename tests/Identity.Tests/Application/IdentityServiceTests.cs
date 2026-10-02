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
    private readonly Mock<IGoogleTokenValidator> _googleTokenValidatorMock;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();

        _jwtTokenServiceMock.Setup(x => x.CreateTokens(It.IsAny<ApplicationUser>()))
            .Returns(new TokenResponse(
                "mock-access-token",
                DateTime.UtcNow.AddMinutes(15),
                "mock-refresh-token",
                DateTime.UtcNow.AddDays(30)));

        _service = new IdentityService(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _googleTokenValidatorMock.Object);
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

    [Fact]
    public async Task SendAndVerifyOtpAsync_ValidPhoneAndOtp_ReturnsAuthResponse()
    {
        // Arrange
        var phone = "9876543210";
        await _service.SendOtpAsync(phone);
        var code = IdentityService.GetDevOtpForTesting(phone)!;

        // Act
        var result = await _service.VerifyOtpAsync(phone, code, "Web");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("9876543210", result.User.PhoneNumber);
        Assert.True(result.User.PhoneNumberVerified);
        Assert.Equal("mock-access-token", result.Tokens.AccessToken);
    }

    [Fact]
    public async Task VerifyOtpAsync_ExceedingMaxFailedAttempts_InvalidatesOtpAndRejectsSubsequentValidCode()
    {
        // Arrange
        var phone = "9112233445";
        await _service.SendOtpAsync(phone);
        var validCode = IdentityService.GetDevOtpForTesting(phone)!;
        Assert.NotNull(validCode);

        // Act: Fail 5 times
        for (var i = 0; i < 5; i++)
        {
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.VerifyOtpAsync(phone, "000000", "Web"));
            Assert.Equal("Invalid or expired OTP code.", ex.Message);
        }

        // Assert: 6th attempt with previously valid code must fail because OTP was pruned/invalidated
        var finalEx = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.VerifyOtpAsync(phone, validCode, "Web"));
        Assert.Equal("Invalid or expired OTP code.", finalEx.Message);
    }

    // =========================================================================
    // Google Sign-In Unit Tests (Section 22)
    // =========================================================================

    [Fact]
    public async Task GoogleLoginAsync_ValidToken_NewGoogleUser_CreatesUserExternalLoginAndIssuesTokens()
    {
        // Arrange
        var payload = new GoogleUserPayload(
            Subject: "google-sub-1001",
            Email: "newuser@gmail.com",
            EmailVerified: true,
            Name: "Aarav Verma",
            GivenName: "Aarav",
            FamilyName: "Verma",
            PictureUrl: "https://lh3.googleusercontent.com/a/aarav"
        );

        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("valid-google-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        _userRepositoryMock
            .Setup(x => x.GetByExternalLoginAsync("Google", "google-sub-1001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _userRepositoryMock
            .Setup(x => x.FindExternalLoginAsync("Google", "google-sub-1001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExternalLogin?)null);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("newuser@gmail.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.GoogleLoginAsync("valid-google-token", "Chrome");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Aarav Verma", result.User.FullName);
        Assert.Equal("Aarav", result.User.FirstName);
        Assert.Equal("Verma", result.User.LastName);
        Assert.Equal("newuser@gmail.com", result.User.Email);
        Assert.Equal("https://lh3.googleusercontent.com/a/aarav", result.User.ProfilePictureUrl);
        Assert.True(result.User.EmailVerified);
        Assert.Equal("mock-access-token", result.Tokens.AccessToken);
        Assert.Equal("mock-refresh-token", result.Tokens.RefreshToken);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(
            x => x.AddExternalLoginAsync(
                It.Is<ExternalLogin>(el => el.Provider == "Google" && el.ProviderUserId == "google-sub-1001"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepositoryMock.Verify(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GoogleLoginAsync_ExistingGoogleUser_ReturnsSameUserAndIssuesTokensWithoutCreatingDuplicate()
    {
        // Arrange
        var existingUser = ApplicationUser.CreateExternalUser(
            Guid.NewGuid(),
            "Priya Nair",
            "priya@gmail.com",
            emailVerified: true,
            firstName: "Priya",
            lastName: "Nair");
        var existingLogin = new ExternalLogin(Guid.NewGuid(), existingUser.Id, "Google", "google-sub-2002", "priya@gmail.com");
        existingUser.AddExternalLogin(existingLogin);

        var payload = new GoogleUserPayload(
            Subject: "google-sub-2002",
            Email: "priya@gmail.com",
            EmailVerified: true,
            Name: "Priya Nair",
            GivenName: "Priya",
            FamilyName: "Nair",
            PictureUrl: "https://lh3.googleusercontent.com/a/priya"
        );

        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("existing-google-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        _userRepositoryMock
            .Setup(x => x.GetByExternalLoginAsync("Google", "google-sub-2002", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.GoogleLoginAsync("existing-google-token", "Chrome");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingUser.Id, result.User.Id);
        Assert.Equal("priya@gmail.com", result.User.Email);
        Assert.Equal("mock-access-token", result.Tokens.AccessToken);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddExternalLoginAsync(It.IsAny<ExternalLogin>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GoogleLoginAsync_ExistingUserWithSameVerifiedEmail_LinksGoogleIdentityToExistingAccount()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("P@ssword123");
        var existingUser = new ApplicationUser(
            Guid.NewGuid(),
            "Rohan Gupta",
            "9876543210",
            passwordHash,
            "rohan@gmail.com");

        var payload = new GoogleUserPayload(
            Subject: "google-sub-3003",
            Email: "rohan@gmail.com",
            EmailVerified: true,
            Name: "Rohan Gupta",
            GivenName: "Rohan",
            FamilyName: "Gupta",
            PictureUrl: "https://lh3.googleusercontent.com/a/rohan"
        );

        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("link-google-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        _userRepositoryMock
            .Setup(x => x.GetByExternalLoginAsync("Google", "google-sub-3003", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("rohan@gmail.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.GoogleLoginAsync("link-google-token", "Chrome");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingUser.Id, result.User.Id);
        Assert.True(result.User.EmailVerified);
        Assert.Single(existingUser.ExternalLogins);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(
            x => x.AddExternalLoginAsync(
                It.Is<ExternalLogin>(el => el.UserId == existingUser.Id && el.ProviderUserId == "google-sub-3003"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GoogleLoginAsync_InvalidGoogleToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired Google ID token."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GoogleLoginAsync("invalid-token", "Chrome"));
    }

    [Fact]
    public async Task GoogleLoginAsync_ExpiredGoogleToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("expired-token", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired Google ID token."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GoogleLoginAsync("expired-token", "Chrome"));
    }

    [Fact]
    public async Task GoogleLoginAsync_WrongGoogleAudience_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("wrong-audience-token", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired Google ID token."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GoogleLoginAsync("wrong-audience-token", "Chrome"));
    }

    [Fact]
    public async Task GoogleLoginAsync_UnverifiedEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var unverifiedPayload = new GoogleUserPayload(
            Subject: "google-sub-4004",
            Email: "unverified@gmail.com",
            EmailVerified: false,
            Name: "Unverified User"
        );

        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("unverified-email-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(unverifiedPayload);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GoogleLoginAsync("unverified-email-token", "Chrome"));
    }

    [Fact]
    public async Task GoogleLoginAsync_DuplicateExternalIdentityConflict_ThrowsConflictException()
    {
        // Arrange
        var existingUser = ApplicationUser.CreateExternalUser(
            Guid.NewGuid(),
            "Existing User",
            "conflict@gmail.com",
            emailVerified: true);
        existingUser.AddExternalLogin(
            new ExternalLogin(Guid.NewGuid(), existingUser.Id, "Google", "original-google-sub", "conflict@gmail.com"));

        var conflictingPayload = new GoogleUserPayload(
            Subject: "different-google-sub",
            Email: "conflict@gmail.com",
            EmailVerified: true,
            Name: "Conflicting User"
        );

        _googleTokenValidatorMock
            .Setup(x => x.ValidateAsync("conflict-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingPayload);

        _userRepositoryMock
            .Setup(x => x.GetByExternalLoginAsync("Google", "different-google-sub", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("conflict@gmail.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.GoogleLoginAsync("conflict-token", "Chrome"));
    }
}
