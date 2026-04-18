using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SocialLogin.Application.Common.Interfaces;
using SocialLogin.Infrastructure.Options;

namespace SocialLogin.Infrastructure.Services;

public sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthOptions _options;

    public GoogleAuthService(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                payload.GivenName ?? string.Empty,
                payload.FamilyName ?? string.Empty,
                payload.Picture);
        }
        catch
        {
            return null;
        }
    }
}
