using SocialLogin.Domain.Common;
using SocialLogin.Domain.Common.Errors;

namespace SocialLogin.Domain.Errors;

public static class DomainErrors
{
    public static class Auth
    {
        public static Error InvalidGoogleToken =>
            new InvalidCredentialsError(["The Google token is invalid or expired."], new NoException());

        public static Error RefreshTokenNotFound =>
            new NotFoundError(["The refresh token was not found."], new NoException());

        public static Error RefreshTokenExpired =>
            new InvalidCredentialsError(["The refresh token has expired."], new NoException());

        public static Error RefreshTokenRevoked =>
            new InvalidCredentialsError(["The refresh token has been revoked."], new NoException());

        public static Error RefreshTokenAlreadyUsed =>
            new InvalidCredentialsError(["The refresh token has already been used."], new NoException());

        public static Error UserNotFound =>
            new UserNotFoundError(["User was not found."], new NoException());

        public static Error UserCreationFailed =>
            new UnexpectedError(["Failed to create user account."], new NoException());
    }
}
