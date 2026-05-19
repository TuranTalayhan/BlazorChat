using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class FriendshipRepository(AppDbContext db) : IFriendshipRepository
{
    public async Task<string?> GetUsernameByIdAsync(int userId, CancellationToken ct)
    {
        return await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<bool> FriendshipExistsAsync(int userId1, int userId2, CancellationToken ct)
    {
        return await db.Friendships.AnyAsync(f =>
            (f.RequesterId == userId1 && f.ReceiverId == userId2) ||
            (f.RequesterId == userId2 && f.ReceiverId == userId1), ct);
    }

    public async Task AddAsync(Friendship friendship, CancellationToken ct)
    {
        await db.Friendships.AddAsync(friendship, ct);
    }
    
    public async Task<Friendship?> GetPendingRequestAsync(int requesterId, int receiverId, CancellationToken ct)
    {
        return await db.Friendships
            .Include(f => f.Requester)
            .FirstOrDefaultAsync(f => f.RequesterId == requesterId 
                                      && f.ReceiverId == receiverId 
                                      && f.Status == FriendshipStatus.Pending, ct);
    }

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public void Remove(Friendship friendship)
    {
        db.Friendships.Remove(friendship);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
    
    public async Task<List<PendingFriendshipDto>> GetPendingRequestsAsync(int userId, CancellationToken ct)
    {
        return await db.Friendships
            .AsNoTracking()
            .Where(f => f.ReceiverId == userId && f.Status == FriendshipStatus.Pending)
            .Select(f => new PendingFriendshipDto
            {
                RequesterId = f.RequesterId,
                RequesterUsername = f.Requester.Username,
                CreatedAt = f.CreatedAt
            })
            .OrderByDescending(f => f.CreatedAt) 
            .ToListAsync(ct);
    }
    
    public async Task<List<SidebarFriendSummaryDto>> GetFriendsSummaryAsync(int userId, CancellationToken ct)
    {
        var friendIdsQuery = db.Friendships
            .AsNoTracking()
            .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId);

        return await db.Users
            .AsNoTracking()
            .Where(u => friendIdsQuery.Contains(u.Id))
            .Select(u => new
            {
                User = u,
                Channel = db.Channels.FirstOrDefault(c => 
                    c.Type == ChannelType.DirectMessage && 
                    c.Members.Any(m => m.Id == userId) && 
                    c.Members.Any(m => m.Id == u.Id)),
            })
            .Select(x => new SidebarFriendSummaryDto(
                x.User.Id,
                x.User.Username,
                x.User.AvatarUrl,
                x.User.Status,
                x.Channel != null ? x.Channel.Id : 0,
                x.Channel != null && x.Channel.Messages.Max(m => (int?)m.Id) > db.UserChannelStates
                    .Where(ucs => ucs.UserId == userId && ucs.ChannelId == x.Channel.Id)
                    .Select(ucs => ucs.LastReadMessageId)
                    .FirstOrDefault()
            ))
            .ToListAsync(ct);
    }
    
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