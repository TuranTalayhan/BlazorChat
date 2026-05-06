namespace BlazorChat.Server.Application.Features.Messages;

public record MessageResult<T>(bool IsSuccess, T? Data = default, MessageError Error = MessageError.None, string? ErrorMessage = null);