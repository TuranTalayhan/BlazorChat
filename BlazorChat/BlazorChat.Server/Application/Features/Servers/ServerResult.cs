namespace BlazorChat.Server.Application.Features.Servers;

public record ServerResult<T>(bool IsSuccess, T? Data = default, ServerError Error = ServerError.None);