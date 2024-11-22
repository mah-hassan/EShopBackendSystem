namespace EShop.Domain.Shared.Errors;

public enum ErrorType
{
    BadRequest = 0,
    Conflict = 1,
    NotFound = 2,
    Forbidden = 4,
    InternalServerError = 6,
}