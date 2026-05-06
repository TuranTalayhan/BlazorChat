using System.Security.Claims;
using BlazorChat.Server.Context;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Features.Channels;

public record GetOrCreateDmCommand(int FriendId) : IRequest<int>;

public class GetOrCreateDmHandler(AppDbContext db, IUserContext userContext) 
    : IRequestHandler<GetOrCreateDmCommand, int>
{
    public async ValueTask<int> Handle(GetOrCreateDmCommand request, CancellationToken ct)
    {
        var currentUserId = userContext.UserId;

        // 1. Try to find existing DM
        var existingId = await db.Channels
            .Where(c => c.Type == ChannelType.DirectMessage)
            .Where(c => c.Members.Any(m => m.Id == currentUserId) && 
                        c.Members.Any(m => m.Id == request.FriendId))
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);

        if (existingId > 0) return existingId;

        // 2. Create new DM
        var currentUser = await db.Users.FirstAsync(u => u.Id == currentUserId, ct);
        var friendUser = await db.Users.FirstOrDefaultAsync(u => u.Id == request.FriendId, ct)
                         ?? throw new Exception("Friend not found");

        var channel = new Channel
        {
            Type = ChannelType.DirectMessage,
            Members = new List<User> { currentUser, friendUser }
        };

        db.Channels.Add(channel);
        await db.SaveChangesAsync(ct);

        return channel.Id;
    }
}