namespace Campanhas.Api.Services;

public readonly record struct Result<T>
{
    public T? Value { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }
    public bool IsSuccess => Error is null;

    public static Result<T> Ok(T value) => new() { Value = value, StatusCode = 200 };
    public static Result<T> Created(T value) => new() { Value = value, StatusCode = 201 };
    public static Result<T> Accepted(T value) => new() { Value = value, StatusCode = 202 };
    public static Result<T> Fail(int statusCode, string error) => new() { StatusCode = statusCode, Error = error };
}
