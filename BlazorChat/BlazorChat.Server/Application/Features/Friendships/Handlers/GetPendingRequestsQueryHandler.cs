using BlazorChat.Server.Application.Features.Friendships.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;


public class GetPendingRequestsQueryHandler(AppDbContext db) 
    : IQueryHandler<GetPendingRequestsQuery, List<PendingFriendshipDto>>
{
    public async ValueTask<List<PendingFriendshipDto>> Handle(GetPendingRequestsQuery request, CancellationToken ct)
    {
        return await db.Friendships
            .Include(f => f.Requester)
            .Where(f => f.ReceiverId == request.CurrentUserId && f.Status == FriendshipStatus.Pending)
            .Select(f => new PendingFriendshipDto
            {
                RequesterId = f.RequesterId,
                RequesterUsername = f.Requester.Username,
                CreatedAt = f.CreatedAt
            })
            .OrderByDescending(f => f.CreatedAt) 
            .ToListAsync(ct);
    }
}
