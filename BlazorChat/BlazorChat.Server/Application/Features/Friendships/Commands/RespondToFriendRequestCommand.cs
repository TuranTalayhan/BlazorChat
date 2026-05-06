using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Commands;

public record RespondToFriendRequestCommand(int CurrentUserId, int RequesterId, bool Accept) : ICommand<FriendshipResult>;