using BlazorChat.Server.Application.Features.Friendships.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class RespondToFriendRequestCommandHandler(AppDbContext db, IFriendNotificationService notifications) 
    : ICommandHandler<RespondToFriendRequestCommand, FriendshipResult>
{
    public async ValueTask<FriendshipResult> Handle(RespondToFriendRequestCommand request, CancellationToken ct)
    {
        var friendship = await db.Friendships.Include(f => f.Requester)
            .FirstOrDefaultAsync(f => f.RequesterId == request.RequesterId 
                                      && f.ReceiverId == request.CurrentUserId 
                                      && f.Status == FriendshipStatus.Pending, ct);

        if (friendship == null) return new FriendshipResult(false, FriendshipError.NotFound, "Friend request not found.");

        if (!request.Accept)
        {
            db.Friendships.Remove(friendship);
            await db.SaveChangesAsync(ct);
            return new FriendshipResult(true);
        }

        var current = await db.Users.FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, ct);
        friendship.Status = FriendshipStatus.Accepted;
        
        await db.SaveChangesAsync(ct);

        await notifications.SendNewFriendAddedAsync(request.CurrentUserId.ToString(), new FriendshipDto {
            FriendId = friendship.RequesterId, Username = friendship.Requester.Username, Status = friendship.Requester.Status
        });
        await notifications.SendNewFriendAddedAsync(request.RequesterId.ToString(), new FriendshipDto {
            FriendId = current!.Id, Username = current.Username, Status = current.Status
        });

        return new FriendshipResult(true);
    }
}