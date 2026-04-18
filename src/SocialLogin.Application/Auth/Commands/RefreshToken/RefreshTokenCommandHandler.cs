using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialLogin.Application.Auth.DTOs;
using SocialLogin.Application.Common.Interfaces;
using SocialLogin.Domain.Common;
using SocialLogin.Domain.Entities;
using SocialLogin.Domain.Errors;

namespace SocialLogin.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IApplicationDbContext _dbContext;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null)
            return Result.Failure<AuthResponse>(DomainErrors.Auth.RefreshTokenNotFound);

        if (storedToken.IsRevoked)
            return Result.Failure<AuthResponse>(DomainErrors.Auth.RefreshTokenRevoked);

        if (storedToken.IsUsed)
            return Result.Failure<AuthResponse>(DomainErrors.Auth.RefreshTokenAlreadyUsed);

        if (DateTime.UtcNow >= storedToken.ExpiresAt)
            return Result.Failure<AuthResponse>(DomainErrors.Auth.RefreshTokenExpired);

        storedToken.IsUsed = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = request.IpAddress;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, request.IpAddress);

        return new AuthResponse(
            newAccessToken,
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(15),
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName);
    }
}
