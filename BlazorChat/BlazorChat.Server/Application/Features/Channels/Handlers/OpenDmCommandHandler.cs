using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class OpenDmCommandHandler(AppDbContext db) : ICommandHandler<OpenDmCommand, ChannelResult<int>>
{
    public async ValueTask<ChannelResult<int>> Handle(OpenDmCommand request, CancellationToken ct)
    {
        if (request.CurrentUserId == request.FriendId)
            return new ChannelResult<int>(false, Error: ChannelError.BadRequest, ErrorMessage: "Cannot DM yourself.");

        var existingDm = await db.Channels
            .Where(c => c.Type == ChannelType.DirectMessage)
            .FirstOrDefaultAsync(c => 
                c.Members.Any(m => m.Id == request.CurrentUserId) && 
                c.Members.Any(m => m.Id == request.FriendId), ct);

        // If it exists, return 200 OK (IsNewChannel = false)
        if (existingDm != null)
            return new ChannelResult<int>(true, Data: existingDm.Id, IsNewChannel: false);

        // 2. It doesn't exist, so fetch the users and create it
        var currentUser = await db.Users.FindAsync([request.CurrentUserId], ct);
        var friendUser = await db.Users.FindAsync([request.FriendId], ct);

        if (currentUser == null || friendUser == null)
            return new ChannelResult<int>(false, Error: ChannelError.NotFound, ErrorMessage: "User not found.");

        var newDm = new Channel
        {
            Type = ChannelType.DirectMessage,
            CreatedAt = DateTime.UtcNow,
            Members = new List<User> { currentUser, friendUser }
        };

        db.Channels.Add(newDm);
        await db.SaveChangesAsync(ct);

        // Return 201 Created (IsNewChannel = true)
        return new ChannelResult<int>(true, Data: newDm.Id, IsNewChannel: true);
    }
}