using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class FriendshipRepository(AppDbContext db) : IFriendshipRepository
{
    public async Task<List<FriendshipDto>> GetAcceptedFriendsAsync(int userId, CancellationToken ct)
    {
        return await db.Friendships
            .AsNoTracking()
            .Where(f => (f.RequesterId == userId || f.ReceiverId == userId)
                        && f.Status == FriendshipStatus.Accepted)
            .Select(f => new FriendshipDto
            {
                FriendId = f.RequesterId == userId ? f.ReceiverId : f.RequesterId,
                Username = f.RequesterId == userId ? f.Receiver.Username : f.Requester.Username,
                Status = f.RequesterId == userId ? f.Receiver.Status : f.Requester.Status
            })
            .ToListAsync(ct);
    }
}