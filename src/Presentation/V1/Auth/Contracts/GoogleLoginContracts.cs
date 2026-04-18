using SocialLogin.Application.Auth.Commands.GoogleLogin;
using SocialLogin.Application.Auth.DTOs;

namespace SocialLogin.Api.V1.Auth.Contracts;

public record GoogleLoginRequest(string IdToken)
{
    public GoogleLoginCommand ToCommand(string? ipAddress) =>
        new(IdToken, ipAddress);
}

public record GoogleLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    string UserId,
    string Email,
    string FirstName,
    string LastName)
{
    public static GoogleLoginResponse FromResult(AuthResponse result) =>
        new(result.AccessToken,
            result.RefreshToken,
            result.AccessTokenExpiry,
            result.UserId,
            result.Email,
            result.FirstName,
            result.LastName);
}
