using SocialLogin.Domain.Entities;

namespace SocialLogin.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string? ipAddress);
}
