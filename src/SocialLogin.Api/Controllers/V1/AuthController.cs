using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialLogin.Api.Common;
using SocialLogin.Api.Extensions;
using SocialLogin.Application.Auth.Commands.GoogleLogin;
using SocialLogin.Application.Auth.Commands.RefreshToken;
using SocialLogin.Application.Auth.Commands.RevokeToken;
using System.Security.Claims;

namespace SocialLogin.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Authenticate with Google and receive JWT tokens.
    /// </summary>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var command = new GoogleLoginCommand(request.IdToken, GetIpAddress());
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Refresh an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Application.Auth.DTOs.AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken, GetIpAddress());
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Revoke a refresh token (logout).
    /// </summary>
    [Authorize]
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(ApiResponse.Fail(new ApiError
            {
                Code = "Auth.Unauthorized",
                Message = "Unable to identify user.",
                StatusCode = 401
            }));

        var command = new RevokeTokenCommand(request.RefreshToken, userId, GetIpAddress());
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    private string? GetIpAddress() =>
        Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIp)
            ? forwardedIp.ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString();
}

public sealed record GoogleLoginRequest(string IdToken);
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
public sealed record RevokeTokenRequest(string RefreshToken);
