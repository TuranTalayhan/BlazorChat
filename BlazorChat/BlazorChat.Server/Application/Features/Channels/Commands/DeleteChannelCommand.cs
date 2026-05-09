using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Commands;

public record DeleteChannelCommand(int CurrentUserId, int ChannelId) : ICommand<ChannelResult<bool>>;