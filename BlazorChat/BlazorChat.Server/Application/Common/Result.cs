namespace BlazorChat.Server.Application.Common;

public enum ErrorType { None, BadRequest, Forbidden, NotFound }

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string ErrorMessage { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string errorMessage, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty, ErrorType.None);
    public static Result<T> Failure(string message, ErrorType type = ErrorType.BadRequest) => new(false, default, message, type);
}