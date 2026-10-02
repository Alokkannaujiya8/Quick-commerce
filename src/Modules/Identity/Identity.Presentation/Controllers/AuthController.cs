namespace Identity.Presentation.Controllers;

using System.Security.Claims;
using BuildingBlocks.Application.Exceptions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AuthController>? _logger;

    public AuthController(
        IIdentityService identityService,
        ILogger<AuthController>? logger = null)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(
            request.FullName,
            request.PhoneNumber,
            request.Email,
            request.Password,
            request.DeviceName,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _identityService.LoginAsync(
                request.Login,
                request.Password,
                request.DeviceName,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path
            });
        }
    }

    [HttpPost("otp/send")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp(
        [FromBody] SendOtpRequest request,
        CancellationToken cancellationToken)
    {
        var success = await _identityService.SendOtpAsync(
            request.PhoneNumber,
            cancellationToken);

        return Ok(new { success });
    }

    [HttpPost("otp/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _identityService.VerifyOtpAsync(
                request.PhoneNumber,
                request.Code,
                request.DeviceName,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path
            });
        }
    }

    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleAuthRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.IdToken))
        {
            _logger?.LogWarning("Google authentication rejected: missing idToken in request.");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Google ID token is required.",
                Instance = HttpContext?.Request?.Path
            });
        }

        try
        {
            var result = await _identityService.GoogleLoginAsync(
                request.IdToken,
                "Google Sign-In",
                cancellationToken);

            _logger?.LogInformation("Google authentication succeeded for UserId {UserId}.", result.User.Id);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger?.LogWarning("Google authentication validation failed: {Reason}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path
            });
        }
        catch (UnauthorizedAccessException ex) when (
            ex.Message.Contains("not active", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("blocked", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("suspended", StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogWarning("Google authentication forbidden for inactive/blocked user: {Reason}", ex.Message);
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.LogWarning("Google authentication unauthorized: {Reason}", ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path
            });
        }
        catch (ConflictException ex)
        {
            _logger?.LogWarning("Google authentication account conflict: {Reason}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error occurred during Google authentication.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing Google authentication.",
                Instance = HttpContext?.Request?.Path
            });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _identityService.RefreshTokenAsync(
                request.RefreshToken,
                request.DeviceName,
                cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = ex.Message,
                Instance = HttpContext?.Request?.Path
            });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        await _identityService.LogoutAsync(
            userId,
            request.RefreshToken,
            cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var user = await _identityService.GetUserAsync(
            userId,
            cancellationToken);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("User identity is invalid or missing.");
        }

        return userId;
    }
}
