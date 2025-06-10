namespace ECommerce.Application.Utility;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; private set; }
    public string? Error { get; private set; }

    public static Result<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static Result<T> Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; private set; }

    public static Result Success() => new()
    {
        IsSuccess = true
    };

    public static Result Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}