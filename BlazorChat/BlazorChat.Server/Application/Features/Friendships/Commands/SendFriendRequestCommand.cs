using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Commands;

public record SendFriendRequestCommand(int CurrentUserId, string TargetUsername) : ICommand<FriendshipResult>;