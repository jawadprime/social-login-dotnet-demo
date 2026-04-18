using SocialLogin.Application.Auth.Commands.RevokeToken;

namespace SocialLogin.Api.V1.Auth.Contracts;

public record RevokeTokenRequest(string RefreshToken)
{
    public RevokeTokenCommand ToCommand(string userId, string? ipAddress) =>
        new(RefreshToken, userId, ipAddress);
}
