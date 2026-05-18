using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Commands;

public record MarkChannelReadCommand(int UserId, int ChannelId, int LastMessageId) 
    : ICommand<CommandResult>;