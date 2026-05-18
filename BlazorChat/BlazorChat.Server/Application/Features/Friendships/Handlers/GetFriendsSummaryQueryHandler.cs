using BlazorChat.Server.Application.Features.Friendships.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class GetFriendsSummaryQueryHandler(AppDbContext db) : IQueryHandler<GetFriendsSummaryQuery, List<SidebarFriendSummaryDto>>
{
    public async ValueTask<List<SidebarFriendSummaryDto>> Handle(GetFriendsSummaryQuery request, CancellationToken ct)
    {
        var friendIds = await db.Friendships
            .Where(f => (f.RequesterId == request.UserId || f.ReceiverId == request.UserId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == request.UserId ? f.ReceiverId : f.RequesterId)
            .ToListAsync(ct);

        var summary = await db.Users
            .Where(u => friendIds.Contains(u.Id))
            .Select(u => new
            {
                User = u,
                Channel = db.Channels
                    .FirstOrDefault(c => c.ServerId == null && c.Members.Any(m => m.Id == request.UserId) && c.Members.Any(m => m.Id == u.Id)),
            })
            .Select(x => new SidebarFriendSummaryDto(
                x.User.Id,
                x.User.Username,
                x.User.AvatarUrl,
                x.User.Status,
                x.Channel != null ? x.Channel.Id : 0,
                x.Channel != null && x.Channel.Messages.Max(m => (int?)m.Id) > db.UserChannelStates
                    .Where(ucs => ucs.UserId == request.UserId && ucs.ChannelId == x.Channel.Id)
                    .Select(ucs => ucs.LastReadMessageId)
                    .FirstOrDefault()
            ))
            .ToListAsync(ct);

        return summary;
    }
}