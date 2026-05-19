using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Commands;

public record GetOrCreateDmCommand(int FriendId) : ICommand<ChannelResult<int>>;
