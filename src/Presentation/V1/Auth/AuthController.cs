using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialLogin.Api.Common;
using SocialLogin.Api.Extensions;
using SocialLogin.Api.V1.Auth.Contracts;
using System.Security.Claims;

namespace ApiPresentation.V1.Auth;

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
    [ProducesResponseType(typeof(GoogleLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemInfo), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemInfo), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _sender.Send(request.ToCommand(GetIpAddress()));
        return result.ToActionResult(GoogleLoginResponse.FromResult);
    }

    /// <summary>
    /// Refresh an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemInfo), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _sender.Send(request.ToCommand(GetIpAddress()));
        return result.ToActionResult(RefreshTokenResponse.FromResult);
    }

    /// <summary>
    /// Revoke a refresh token (logout).
    /// </summary>
    [Authorize]
    [HttpPost("revoke-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemInfo), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemInfo), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new ProblemInfo("Invalid Credentials", "Unable to identify user."));

        var result = await _sender.Send(request.ToCommand(userId, GetIpAddress()));
        return result.ToActionResult();
    }

    private string? GetIpAddress() =>
        Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIp)
            ? forwardedIp.ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString();
}
