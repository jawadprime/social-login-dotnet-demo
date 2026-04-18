using SocialLogin.Application.Auth.Commands.RefreshToken;
using SocialLogin.Application.Auth.DTOs;

namespace SocialLogin.Api.V1.Auth.Contracts;

public record RefreshTokenRequest(string AccessToken, string RefreshToken)
{
    public RefreshTokenCommand ToCommand(string? ipAddress) =>
        new(AccessToken, RefreshToken, ipAddress);
}

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    string UserId,
    string Email,
    string FirstName,
    string LastName)
{
    public static RefreshTokenResponse FromResult(AuthResponse result) =>
        new(result.AccessToken,
            result.RefreshToken,
            result.AccessTokenExpiry,
            result.UserId,
            result.Email,
            result.FirstName,
            result.LastName);
}
