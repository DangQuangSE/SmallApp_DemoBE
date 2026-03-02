namespace SecondBike.Application.Common;

/// <summary>
/// Standard API result wrapper. Follows the Result pattern to avoid exceptions for expected failures.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
    public static Result<T> Failure(List<string> errors) => new() { IsSuccess = false, Errors = errors, ErrorMessage = errors.FirstOrDefault() };
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
