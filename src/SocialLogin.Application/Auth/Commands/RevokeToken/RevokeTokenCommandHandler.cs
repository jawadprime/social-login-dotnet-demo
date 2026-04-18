using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialLogin.Application.Common.Interfaces;
using SocialLogin.Domain.Common;
using SocialLogin.Domain.Errors;

namespace SocialLogin.Application.Auth.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public RevokeTokenCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.Token == request.RefreshToken && t.UserId == request.UserId,
                cancellationToken);

        if (token is null)
            return Result.Failure(DomainErrors.Auth.RefreshTokenNotFound);

        if (token.IsRevoked)
            return Result.Failure(DomainErrors.Auth.RefreshTokenRevoked);

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = request.IpAddress;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
