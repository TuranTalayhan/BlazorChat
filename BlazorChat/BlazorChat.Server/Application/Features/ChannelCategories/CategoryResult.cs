namespace BlazorChat.Server.Application.Features.ChannelCategories;

public record CategoryResult<T>(
    bool IsSuccess, 
    T? Data = default, 
    CategoryError Error = CategoryError.None, 
    string? ErrorMessage = null
)
{
    public static CategoryResult<T> Success(T data) => 
        new(true, Data: data);

    public static CategoryResult<T> Failure(CategoryError error, string? message = null) => 
        new(false, Error: error, ErrorMessage: message);
}

public record CategoryResult(
    bool IsSuccess, 
    CategoryError Error = CategoryError.None, 
    string? ErrorMessage = null
)
{
    public static CategoryResult Success() => new(true);
    public static CategoryResult Failure(CategoryError error, string? message = null) => 
        new(false, Error: error, ErrorMessage: message);
}