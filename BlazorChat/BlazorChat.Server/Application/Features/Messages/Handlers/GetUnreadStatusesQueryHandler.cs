using BlazorChat.Server.Application.Features.Messages.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class GetUnreadStatusesQueryHandler(AppDbContext db) : IQueryHandler<GetUnreadStatusesQuery, List<ChannelUnreadStatusDto>>
{
    public async ValueTask<List<ChannelUnreadStatusDto>> Handle(GetUnreadStatusesQuery request, CancellationToken ct)
    {
        // Find all channels where the newest message ID is strictly greater than the user's last read message ID
        var unreadChannels = await db.Channels
            .Select(c => new
            {
                ChannelId = c.Id,
                LatestMessageId = c.Messages.Max(m => (int?)m.Id) ?? 0,
                LastReadId = db.UserChannelStates
                    .Where(ucs => ucs.UserId == request.UserId && ucs.ChannelId == c.Id)
                    .Select(ucs => ucs.LastReadMessageId)
                    .FirstOrDefault()
            })
            .Select(x => new ChannelUnreadStatusDto
                {
                    ChannelId = x.ChannelId,
                    HasUnreadMessages = x.LatestMessageId > x.LastReadId
                }
            )
            .ToListAsync(ct);

        return unreadChannels;
    }
}