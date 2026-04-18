namespace SocialLogin.Application.Auth.DTOs;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    string UserId,
    string Email,
    string FirstName,
    string LastName
);
