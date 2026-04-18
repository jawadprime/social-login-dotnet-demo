namespace SocialLogin.Domain.Common;

public abstract record MaybeException;
public sealed record HasException(Exception Exception) : MaybeException;
public sealed record NoException() : MaybeException;
