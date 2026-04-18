namespace SocialLogin.Domain.Common.Errors;

public record InvalidCredentialsError(IReadOnlyList<string> Failures, MaybeException MaybeException)
    : HasError("Invalid Credentials", Failures, MaybeException);

public record UserNotFoundError(IReadOnlyList<string> Failures, MaybeException MaybeException)
    : HasError("User Not Found", Failures, MaybeException);

public record RoleNotFoundError(IReadOnlyList<string> Failures, MaybeException MaybeException)
    : HasError("Role Not Found", Failures, MaybeException);

public record UserNotActiveError(IReadOnlyList<string> Failures, MaybeException MaybeException)
    : HasError("User Not Active", Failures, MaybeException);

public record UserLockedOutError(IReadOnlyList<string> Failures, MaybeException MaybeException)
    : HasError("User Locked Out", Failures, MaybeException);
