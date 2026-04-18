using MediatR;
using Microsoft.AspNetCore.Identity;
using SocialLogin.Application.Auth.DTOs;
using SocialLogin.Application.Common.Interfaces;
using SocialLogin.Domain.Common;
using SocialLogin.Domain.Entities;
using SocialLogin.Domain.Errors;

namespace SocialLogin.Application.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public GoogleLoginCommandHandler(
        IGoogleAuthService googleAuthService,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _googleAuthService = googleAuthService;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
        if (googleUser is null)
            return Result.Failure<AuthResponse>(DomainErrors.Auth.InvalidGoogleToken);

        var user = await _userManager.FindByEmailAsync(googleUser.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = googleUser.Email,
                UserName = googleUser.Email,
                FirstName = googleUser.FirstName,
                LastName = googleUser.LastName,
                ProfilePictureUrl = googleUser.ProfilePictureUrl,
                GoogleId = googleUser.GoogleId,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return Result.Failure<AuthResponse>(DomainErrors.Auth.UserCreationFailed);

            await _userManager.AddToRoleAsync(user, "User");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, request.IpAddress);

        return new AuthResponse(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(15),
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName);
    }
}
