using FluentValidation;
using MediatR;
using SocialLogin.Domain.Common;
using SocialLogin.Domain.Common.Errors;

namespace SocialLogin.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var messages = failures
            .Select(f => f.ErrorMessage)
            .ToList();

        var validationError = new ValidationError(messages, new NoException());
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(valueType);
            return (TResponse)Activator.CreateInstance(resultType, validationError)!;
        }

        if (responseType == typeof(Result))
            return (TResponse)(object)Result.Failure(validationError);

        throw new ValidationException(failures);
    }
}
