using EShop.Domain.Shared;
using EShop.Domain.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Api.Extentions;

public static class ResultExtentions
{

    public static IResult ToResponse(this Result result)
    {
        if (result.IsFailure)
        {           
            return result.Errors.First()?.Type switch
            {
                ErrorType.NotFound => Results.NotFound(result),
                ErrorType.Conflict => Results.Conflict(result),
                ErrorType.Forbidden => Results.Forbid(),
                ErrorType.InternalServerError => Results.StatusCode(StatusCodes.Status500InternalServerError),
                _ => Results.BadRequest(result)
            };
        }
        return Results.Ok(result);
    }
}