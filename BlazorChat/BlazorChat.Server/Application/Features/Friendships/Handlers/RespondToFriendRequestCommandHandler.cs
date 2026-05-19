using BlazorChat.Server.Application.Features.Friendships.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class RespondToFriendRequestCommandHandler(
    IFriendshipRepository friendshipRepository, 
    IFriendNotificationService notifications) 
    : ICommandHandler<RespondToFriendRequestCommand, FriendshipResult>
{
    public async ValueTask<FriendshipResult> Handle(RespondToFriendRequestCommand request, CancellationToken ct)
    {
        var friendship = await friendshipRepository.GetPendingRequestAsync(request.RequesterId, request.CurrentUserId, ct);
        if (friendship == null) 
        {
            return new FriendshipResult(false, FriendshipError.NotFound, "Friend request not found.");
        }

        if (!request.Accept)
        {
            friendshipRepository.Remove(friendship);
            await friendshipRepository.SaveChangesAsync(ct);
            return new FriendshipResult(true);
        }

        var current = await friendshipRepository.GetUserByIdAsync(request.CurrentUserId, ct);
        
        friendship.Accept();
        await friendshipRepository.SaveChangesAsync(ct);

        await notifications.SendNewFriendAddedAsync(request.CurrentUserId.ToString(), new FriendshipDto 
        {
            FriendId = friendship.RequesterId, 
            Username = friendship.Requester.Username, 
            Status = friendship.Requester.Status
        });
        
        await notifications.SendNewFriendAddedAsync(request.RequesterId.ToString(), new FriendshipDto 
        {
            FriendId = current!.Id, 
            Username = current.Username, 
            Status = current.Status
        });

        return new FriendshipResult(true);
    }
}