using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class DeleteChannelCommandHandler(AppDbContext db) : ICommandHandler<DeleteChannelCommand, ChannelResult<bool>>
{
    public async ValueTask<ChannelResult<bool>> Handle(DeleteChannelCommand request, CancellationToken ct)
    {
        var channel = await db.Channels
            .FirstOrDefaultAsync(c => c.Id == request.ChannelId, ct);

        if (channel == null)
            return new ChannelResult<bool>(false, Error: ChannelError.NotFound);

        if (channel.Type == ChannelType.DirectMessage)
            return new ChannelResult<bool>(false, Error: ChannelError.BadRequest, ErrorMessage: "Cannot delete a Direct Message this way.");

        var hasPermission = await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == channel.ServerId 
                            && sm.UserId == request.CurrentUserId 
                            && sm.Role != Infrastructure.Persistence.Entities.ServerRole.Member, ct);

        if (!hasPermission)
            return new ChannelResult<bool>(false, Error: ChannelError.Forbidden);

        // Note: Because of foreign keys, you may need to ensure cascade deletion 
        // is set up in EF Core so deleting a channel also deletes its messages!
        db.Channels.Remove(channel);
        await db.SaveChangesAsync(ct);

        return new ChannelResult<bool>(true, Data: true);
    }
}