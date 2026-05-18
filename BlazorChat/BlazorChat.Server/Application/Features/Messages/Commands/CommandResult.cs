namespace BlazorChat.Server.Application.Features.Messages.Commands;

public record CommandResult(bool IsSuccess, string? ErrorMessage = null);