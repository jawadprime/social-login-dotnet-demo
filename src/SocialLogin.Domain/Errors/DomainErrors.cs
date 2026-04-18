namespace SocialLogin.Domain.Errors;

public static class DomainErrors
{
    public static class Auth
    {
        public static readonly Common.Error InvalidGoogleToken =
            Common.Error.Unauthorized("Auth.InvalidGoogleToken", "The Google token is invalid or expired.");

        public static readonly Common.Error RefreshTokenNotFound =
            Common.Error.NotFound("Auth.RefreshTokenNotFound", "The refresh token was not found.");

        public static readonly Common.Error RefreshTokenExpired =
            Common.Error.Unauthorized("Auth.RefreshTokenExpired", "The refresh token has expired.");

        public static readonly Common.Error RefreshTokenRevoked =
            Common.Error.Unauthorized("Auth.RefreshTokenRevoked", "The refresh token has been revoked.");

        public static readonly Common.Error RefreshTokenAlreadyUsed =
            Common.Error.Unauthorized("Auth.RefreshTokenAlreadyUsed", "The refresh token has already been used.");

        public static readonly Common.Error UserNotFound =
            Common.Error.NotFound("Auth.UserNotFound", "User was not found.");

        public static readonly Common.Error UserCreationFailed =
            Common.Error.Failure("Auth.UserCreationFailed", "Failed to create user account.");
    }
}
