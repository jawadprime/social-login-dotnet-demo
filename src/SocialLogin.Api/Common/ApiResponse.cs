namespace SocialLogin.Api.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(ApiError error) => new() { Success = false, Error = error };
}

public sealed class ApiResponse
{
    public bool Success { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse Ok() => new() { Success = true };
    public static ApiResponse Fail(ApiError error) => new() { Success = false, Error = error };
}

public sealed class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
}
