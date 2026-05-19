using BlazorChat.Server.Application.Features.Friendships.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class SendFriendRequestCommandHandler(AppDbContext db, IFriendNotificationService notifications) 
    : ICommandHandler<SendFriendRequestCommand, FriendshipResult>
{
    public async ValueTask<FriendshipResult> Handle(SendFriendRequestCommand request, CancellationToken ct)
    {
        var currentName = await db.Users
            .Where(u => u.Id == request.CurrentUserId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(ct);
            
        if (currentName == null) return new FriendshipResult(false, FriendshipError.NotFound, "Current user not found.");

        var target = await db.Users.FirstOrDefaultAsync(u => u.Username == request.TargetUsername, ct);
        if (target == null) return new FriendshipResult(false, FriendshipError.NotFound, "User not found.");

        if (target.Id == request.CurrentUserId) 
            return new FriendshipResult(false, FriendshipError.BadRequest, "You cannot add yourself as a friend.");

        var exists = await db.Friendships.AnyAsync(f =>
            (f.RequesterId == request.CurrentUserId && f.ReceiverId == target.Id) ||
            (f.RequesterId == target.Id && f.ReceiverId == request.CurrentUserId), ct);

        if (exists) return new FriendshipResult(false, FriendshipError.Conflict, "Relationship already exists.");

        db.Friendships.Add(new Friendship
        {
            RequesterId = request.CurrentUserId,
            ReceiverId = target.Id,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

        // Notify the target user cleanly
        var dto = new PendingFriendshipDto
        {
            RequesterId = request.CurrentUserId,
            RequesterUsername = currentName,
            CreatedAt = DateTime.UtcNow,
        };
        await notifications.SendFriendRequestAsync(target.Id.ToString(), dto);

        return new FriendshipResult(true);
    }
}