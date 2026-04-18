namespace SocialLogin.Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken);
}

public sealed record GoogleUserInfo(
    string GoogleId,
    string Email,
    string FirstName,
    string LastName,
    string? ProfilePictureUrl
);
