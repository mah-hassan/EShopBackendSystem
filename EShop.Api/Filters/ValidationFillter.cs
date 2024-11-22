using EShop.Domain.Shared;
using EShop.Domain.Shared.Errors;
using FluentValidation;

namespace EShop.Api.Filters;

public sealed class ValidationFilter<T>(IValidator<T> validator)
    : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validatableEntry = context.Arguments
            .Where(x => x?.GetType() == typeof(T))
            .FirstOrDefault() as T;

        if (validatableEntry is null)
        {
            return Results
                .BadRequest(
                Result.Failure(new Error($"{typeof(T).Name}", $"{typeof(T).Name} is required")));
        }

        var validationResult = validator.Validate(validatableEntry);

        if (!validationResult.IsValid)
        {
            List<Error> errors = validationResult
                            .Errors
                            .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                            .ToList();

            return Results.BadRequest(Result.Failure(errors));
        }
        return await next(context);
    }
}