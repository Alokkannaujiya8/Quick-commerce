namespace Identity.Tests.Domain;

using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Xunit;

public class ApplicationUserTests
{
    [Fact]
    public void Constructor_ValidArguments_CreatesActiveUser()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fullName = "John Doe";
        var phone = "+919876543210";
        var email = "john@example.com";
        var passwordHash = "hashedpassword";

        // Act
        var user = new ApplicationUser(id, fullName, phone, passwordHash, email);

        // Assert
        Assert.Equal(id, user.Id);
        Assert.Equal("John Doe", user.FullName);
        Assert.Equal("+919876543210", user.PhoneNumber);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.False(user.PhoneNumberVerified);
        Assert.False(user.EmailVerified);
        Assert.Empty(user.RefreshTokens);
    }

    [Theory]
    [InlineData("", "9876543210", "hash")]
    [InlineData("John", "", "hash")]
    [InlineData("John", "9876543210", "")]
    public void Constructor_MissingRequiredFields_ThrowsArgumentException(
        string fullName,
        string phone,
        string passwordHash)
    {
        Assert.Throws<ArgumentException>(() =>
            new ApplicationUser(Guid.NewGuid(), fullName, phone, passwordHash, "test@test.com"));
    }

    [Fact]
    public void StatusTransitions_WorkCorrectly()
    {
        // Arrange
        var user = new ApplicationUser(Guid.NewGuid(), "Jane Doe", "9876543211", "hash");

        // Act & Assert Block
        user.Block();
        Assert.Equal(UserStatus.Blocked, user.Status);
        Assert.NotNull(user.UpdatedAt);

        // Act & Assert Activate
        user.Activate();
        Assert.Equal(UserStatus.Active, user.Status);

        // Act & Assert Suspend
        user.Suspend();
        Assert.Equal(UserStatus.Suspended, user.Status);

        // Act & Assert MarkDeleted
        user.MarkDeleted();
        Assert.Equal(UserStatus.Deleted, user.Status);
    }

    [Fact]
    public void AddRefreshToken_AddsTokenToCollection()
    {
        // Arrange
        var user = new ApplicationUser(Guid.NewGuid(), "Jane Doe", "9876543211", "hash");
        var token = new RefreshToken(Guid.NewGuid(), user.Id, "tokenhash123", DateTime.UtcNow.AddDays(7), "Chrome");

        // Act
        user.AddRefreshToken(token);

        // Assert
        Assert.Single(user.RefreshTokens);
        Assert.Contains(token, user.RefreshTokens);
    }
}

