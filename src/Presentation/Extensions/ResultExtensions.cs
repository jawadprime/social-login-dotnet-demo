using Microsoft.AspNetCore.Mvc;
using SocialLogin.Api.Common;
using SocialLogin.Domain.Common;
using SocialLogin.Domain.Common.Errors;

namespace SocialLogin.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToProblemResult<TDomain>(this Result<TDomain> result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        if (result.IsSuccess)
            throw new InvalidOperationException("Use the overload with mapping for success responses.");

        return result.Error switch
        {
            NotFoundError e        => new NotFoundObjectResult(CreateProblem(e)),
            UserNotFoundError e    => new NotFoundObjectResult(CreateProblem(e)),
            RoleNotFoundError e    => new NotFoundObjectResult(CreateProblem(e)),
            ValidationError e      => new BadRequestObjectResult(CreateProblem(e)),
            InvalidCredentialsError e => new UnauthorizedObjectResult(CreateProblem(e)),
            UserNotActiveError e   => new ObjectResult(CreateProblem(e))
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            UserLockedOutError e   => new ObjectResult(CreateProblem(e))
            {
                StatusCode = StatusCodes.Status423Locked
            },
            _ => new ObjectResult(new ProblemInfo("Unexpected Error", "An unhandled error occurred."))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
    }

    public static IActionResult ToActionResult<TDomain, TResponse>(
        this Result<TDomain> result,
        Func<TDomain, TResponse> map)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        if (map is null)
            throw new ArgumentNullException(nameof(map));

        if (result.IsSuccess)
            return new OkObjectResult(map(result.Value));

        return result.ToProblemResult();
    }

    public static IActionResult ToCreatedActionResult<TDomain, TResponse>(
        this Result<TDomain> result,
        Func<TDomain, TResponse> map)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        if (map is null)
            throw new ArgumentNullException(nameof(map));

        if (result.IsSuccess)
            return new ObjectResult(map(result.Value))
            {
                StatusCode = StatusCodes.Status201Created
            };

        return result.ToProblemResult();
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        if (result.IsSuccess)
            return new OkResult();

        return result.Error switch
        {
            NotFoundError e        => new NotFoundObjectResult(CreateProblem(e)),
            UserNotFoundError e    => new NotFoundObjectResult(CreateProblem(e)),
            ValidationError e      => new BadRequestObjectResult(CreateProblem(e)),
            InvalidCredentialsError e => new UnauthorizedObjectResult(CreateProblem(e)),
            UserNotActiveError e   => new ObjectResult(CreateProblem(e))
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            UserLockedOutError e   => new ObjectResult(CreateProblem(e))
            {
                StatusCode = StatusCodes.Status423Locked
            },
            _ => new ObjectResult(new ProblemInfo("Unexpected Error", "An unhandled error occurred."))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
    }

    private static ProblemInfo CreateProblem(HasError error)
        => new(error.Title, string.Join(", ", error.Failures));
}
