namespace SocialLogin.Domain.Common;

public abstract record Error;

public abstract record HasError(
    string Title,
    IReadOnlyList<string> Failures,
    MaybeException Exception) : Error;

public sealed record NoError() : Error;
