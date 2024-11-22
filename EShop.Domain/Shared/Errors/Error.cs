using System.Text.Json.Serialization;

namespace EShop.Domain.Shared.Errors;

public sealed record Error
{
    public Error(string code, string message, ErrorType type = ErrorType.BadRequest)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public string Code { get; init; }
    public string Message { get; init; }
    [JsonIgnore]
    public ErrorType Type { get; set; } = ErrorType.BadRequest;
}