using BlazorChat.Server.Application.Features.Friendships.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class SendFriendRequestCommandHandler(
    IFriendshipRepository friendshipRepository, 
    IFriendNotificationService notifications) 
    : ICommandHandler<SendFriendRequestCommand, FriendshipResult>
{
    public async ValueTask<FriendshipResult> Handle(SendFriendRequestCommand request, CancellationToken ct)
    {
        var currentUsername = await friendshipRepository.GetUsernameByIdAsync(request.CurrentUserId, ct);
        if (currentUsername == null) 
        {
            return new FriendshipResult(false, FriendshipError.NotFound, "Current user not found.");
        }

        var targetUser = await friendshipRepository.GetUserByUsernameAsync(request.TargetUsername, ct);
        if (targetUser == null) 
        {
            return new FriendshipResult(false, FriendshipError.NotFound, "User not found.");
        }

        if (targetUser.Id == request.CurrentUserId) 
        {
            return new FriendshipResult(false, FriendshipError.BadRequest, "You cannot add yourself as a friend.");
        }

        var relationshipExists = await friendshipRepository.FriendshipExistsAsync(request.CurrentUserId, targetUser.Id, ct);
        if (relationshipExists) 
        {
            return new FriendshipResult(false, FriendshipError.Conflict, "Relationship already exists.");
        }

        var friendship = Friendship.CreatePending(request.CurrentUserId, targetUser.Id);
        
        await friendshipRepository.AddAsync(friendship, ct);
        await friendshipRepository.SaveChangesAsync(ct);

        var dto = new PendingFriendshipDto
        {
            RequesterId = request.CurrentUserId,
            RequesterUsername = currentUsername,
            CreatedAt = friendship.CreatedAt
        };
        await notifications.SendFriendRequestAsync(targetUser.Id.ToString(), dto);

        return new FriendshipResult(true);
    }
}