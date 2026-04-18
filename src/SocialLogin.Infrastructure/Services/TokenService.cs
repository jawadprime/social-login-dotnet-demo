using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialLogin.Application.Common.Interfaces;
using SocialLogin.Domain.Entities;
using SocialLogin.Infrastructure.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SocialLogin.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IApplicationDbContext _dbContext;

    public TokenService(IOptions<JwtOptions> jwtOptions, IApplicationDbContext dbContext)
    {
        _jwtOptions = jwtOptions.Value;
        _dbContext = dbContext;
    }

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string? ipAddress)
    {
        var token = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        await _dbContext.RefreshTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();

        return token;
    }
}
