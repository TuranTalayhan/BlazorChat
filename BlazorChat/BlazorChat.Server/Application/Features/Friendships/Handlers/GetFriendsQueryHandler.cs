using BlazorChat.Server.Application.Features.Friendships.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class GetFriendsQueryHandler(AppDbContext db) : IQueryHandler<GetFriendsQuery, List<FriendshipDto>>
{
    public async ValueTask<List<FriendshipDto>> Handle(GetFriendsQuery request, CancellationToken ct)
    {
        return await db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Receiver)
            .Where(f => (f.RequesterId == request.CurrentUserId || f.ReceiverId == request.CurrentUserId)
                        && f.Status == FriendshipStatus.Accepted)
            .Select(f => new FriendshipDto
            {
                FriendId = f.RequesterId == request.CurrentUserId ? f.ReceiverId : f.RequesterId,
                Username = f.RequesterId == request.CurrentUserId ? f.Receiver.Username : f.Requester.Username,
                Status = f.RequesterId == request.CurrentUserId ? f.Receiver.Status : f.Requester.Status
            })
            .ToListAsync(ct);
    }
}