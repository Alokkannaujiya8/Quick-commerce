namespace Identity.Tests.Presentation;

using BuildingBlocks.Application.Exceptions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Presentation.Controllers;
using Identity.Presentation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class AuthControllerGoogleTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly AuthController _controller;

    public AuthControllerGoogleTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _controller = new AuthController(_identityServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GoogleLogin_ValidGoogleToken_Returns200OkAndTokens()
    {
        var expectedResponse = new AuthResponse(
            new UserDto(
                Guid.NewGuid(),
                "Aarav Verma",
                string.Empty,
                "aarav@gmail.com",
                "Active",
                false,
                true,
                "Aarav",
                "Verma",
                "https://lh3.googleusercontent.com/a/aarav"),
            new TokenResponse(
                "jwt-access-token",
                DateTime.UtcNow.AddMinutes(15),
                "refresh-token",
                DateTime.UtcNow.AddDays(30)));

        _identityServiceMock
            .Setup(x => x.GoogleLoginAsync("valid-google-token", "Google Sign-In", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest("valid-google-token"),
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var actual = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("jwt-access-token", actual.Tokens.AccessToken);
        Assert.Equal("refresh-token", actual.Tokens.RefreshToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GoogleLogin_MissingOrEmptyIdToken_Returns400BadRequest(string emptyToken)
    {
        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest(emptyToken),
            CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_InvalidGoogleToken_Returns401Unauthorized()
    {
        _identityServiceMock
            .Setup(x => x.GoogleLoginAsync("invalid-token", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired Google ID token."));

        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest("invalid-token"),
            CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_ExpiredGoogleToken_Returns401Unauthorized()
    {
        _identityServiceMock
            .Setup(x => x.GoogleLoginAsync("expired-token", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Google ID token has expired."));

        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest("expired-token"),
            CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_WrongGoogleAudience_Returns401Unauthorized()
    {
        _identityServiceMock
            .Setup(x => x.GoogleLoginAsync("wrong-aud-token", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Google ID token audience does not match configured Client ID."));

        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest("wrong-aud-token"),
            CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_WrongGoogleIssuer_Returns401Unauthorized()
    {
        _identityServiceMock
            .Setup(x => x.GoogleLoginAsync("wrong-iss-token", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid Google ID token issuer."));

        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest("wrong-iss-token"),
            CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_BlockedOrInactiveUser_Returns403Forbidden()
    {
        _identityServiceMock
            .Setup(x => x.GoogleLoginAsync("blocked-user-token", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User account is not active."));

        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest("blocked-user-token"),
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_AccountLinkingConflict_Returns409Conflict()
    {
        _identityServiceMock
            .Setup(x => x.GoogleLoginAsync("conflict-token", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("User account is already linked to a different Google identity."));

        var result = await _controller.GoogleLogin(
            new GoogleAuthRequest("conflict-token"),
            CancellationToken.None);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
    }
}
