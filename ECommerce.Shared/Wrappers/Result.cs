namespace ECommerce.Shared.Wrappers;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; init; }
    public string? Message { get; init; }

    public static Result<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static Result<T> Failure(string error) => new()
    {
        IsSuccess = false,
        Message = error
    };
}

public class Result
{
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string? Message { get; init; }

    public static Result Success() => new()
    {
        IsSuccess = true
    };

    public static Result Failure(string error) => new()
    {
        IsSuccess = false,
        Message = error
    };
}