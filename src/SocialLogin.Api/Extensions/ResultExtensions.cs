using Microsoft.AspNetCore.Mvc;
using SocialLogin.Api.Common;
using SocialLogin.Domain.Common;

namespace SocialLogin.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse<T>.Ok(result.Value));

        return result.Error.Type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(
                ApiResponse<T>.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 404 })),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(
                ApiResponse<T>.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 401 })),
            ErrorType.Conflict => new ConflictObjectResult(
                ApiResponse<T>.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 409 })),
            ErrorType.Validation => new BadRequestObjectResult(
                ApiResponse<T>.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 400 })),
            _ => new ObjectResult(
                ApiResponse<T>.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 500 }))
            { StatusCode = 500 }
        };
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse.Ok());

        return result.Error.Type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(
                ApiResponse.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 404 })),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(
                ApiResponse.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 401 })),
            ErrorType.Conflict => new ConflictObjectResult(
                ApiResponse.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 409 })),
            ErrorType.Validation => new BadRequestObjectResult(
                ApiResponse.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 400 })),
            _ => new ObjectResult(
                ApiResponse.Fail(new ApiError { Code = result.Error.Code, Message = result.Error.Description, StatusCode = 500 }))
            { StatusCode = 500 }
        };
    }
}
