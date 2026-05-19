using BlazorChat.Server.Application.Features.Users.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Users.Handlers;

public class UpdateUserStatusCommandHandler(AppDbContext db, IUserNotificationService notifications) 
    : ICommandHandler<UpdateUserStatusCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateUserStatusCommand request, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([request.CurrentUserId], ct);
        if (user == null) return false;

        var friendIds = await db.Friendships
            .Where(f => (f.RequesterId == request.CurrentUserId || f.ReceiverId == request.CurrentUserId)
                        && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == request.CurrentUserId ? f.ReceiverId.ToString() : f.RequesterId.ToString())
            .ToListAsync(ct);

        user.Status = request.Dto.Status;
        await db.SaveChangesAsync(ct);

        var statusMsg = new ReceiveUserStatusDto
        {
            Id = request.CurrentUserId,
            Status = request.Dto.Status
        };
        await notifications.SendUserStatusChangedAsync(friendIds, statusMsg);

        return true;
    }
}