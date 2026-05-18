using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class MarkChannelReadCommandHandler(AppDbContext db) : ICommandHandler<MarkChannelReadCommand, CommandResult>
{
    public async ValueTask<CommandResult> Handle(MarkChannelReadCommand request, CancellationToken ct)
    {
        var channelExists = await db.Channels.AnyAsync(c => c.Id == request.ChannelId, ct);
        if (!channelExists)
        {
            return new CommandResult(false, "Channel not found.");
        }

        var messageExists = await db.Messages.AnyAsync(m => m.Id == request.LastMessageId && m.ChannelId == request.ChannelId, ct);
        if (!messageExists)
        {
            return new CommandResult(false, "Invalid message ID for this channel.");
        }

        var existingState = await db.UserChannelStates
            .FirstOrDefaultAsync(ucs => ucs.UserId == request.UserId && ucs.ChannelId == request.ChannelId, ct);

        if (existingState != null)
        {
            if (request.LastMessageId > existingState.LastReadMessageId)
            {
                existingState.LastReadMessageId = request.LastMessageId;
            }
        }
        else
        {
            var newState = new UserChannelState
            {
                UserId = request.UserId,
                ChannelId = request.ChannelId,
                LastReadMessageId = request.LastMessageId
            };
            db.UserChannelStates.Add(newState);
        }

        await db.SaveChangesAsync(ct);
        return new CommandResult(true);
    }
}