using Microsoft.EntityFrameworkCore;
using SocialLogin.Domain.Entities;

namespace SocialLogin.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
