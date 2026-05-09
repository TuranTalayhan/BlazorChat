using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Commands;

public record OpenDmCommand(int CurrentUserId, int FriendId) : ICommand<ChannelResult<int>>;