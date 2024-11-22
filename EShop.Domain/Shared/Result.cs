using EShop.Domain.Shared.Errors;
using System.Text;

namespace EShop.Domain.Shared;

public class Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public List<Error> Errors { get; init; } = new();

    protected Result() { }
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Success Result can not have errors");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("Failure Result can not be empty must contains errors");

        IsSuccess = isSuccess;
        if (error is not null)
            Errors.Add(error);
    }

    protected Result(List<Error> errors)
    {
        if (errors is null || !errors.Any())
            throw new InvalidOperationException("Failure Result can not be empty must contains errors");
        IsSuccess = false;
        Errors.AddRange(errors);
    }

    public static Result Success() => new(true, default);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(List<Error> errors) => new(errors);

    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(true, default, value);
    }

    public static Result<TValue> Failure<TValue>(Error error)
    {
        return new Result<TValue>(false, error);
    }
    public static Result<TValue> Failure<TValue>(List<Error> errors) => new(errors);


    public override string ToString()
    {
        var resultBuilder = new StringBuilder();
        resultBuilder.AppendLine($"\tStatus: {(IsSuccess ? "Success" : "Failure")}");
        foreach (var error in Errors)
        {
            resultBuilder.AppendLine($"{error.Code}: {error.Message}");
        }
        return resultBuilder.ToString();
    }
}

public class Result<TValue> : Result
{
    public TValue? Value { get; init; }
    internal Result(bool isSuccess, Error? error, TValue? value = default)
        : base(isSuccess, error)
    {
        Value = value;
    }
    internal Result(List<Error> errors)
        : base(errors)
    {
       
    }
    private Result() { }
    public static implicit operator Result<TValue>(TValue value)
        => new Result<TValue>(true, default, value);

    public override string ToString()
    {
        var resultBuilder = new StringBuilder(base.ToString());
        if (Value is not null)
            resultBuilder.AppendLine($"Value: {Value}");
        return resultBuilder.ToString();
    }
}