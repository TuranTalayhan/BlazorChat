using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct)
    {
        return await db.Users.FindAsync([userId], ct);
    }

    public async Task<List<string>> GetAcceptedFriendIdsAsync(int userId, CancellationToken ct)
    {
        return await db.Friendships
            .AsNoTracking()
            .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) 
                        && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == userId ? f.ReceiverId.ToString() : f.RequesterId.ToString())
            .ToListAsync(ct);
    }
    
    public async Task<List<string>> SearchUsernamesAsync(int currentUserId, string searchTerm, int limit, CancellationToken ct)
    {
        var normalized = searchTerm.ToLower();

        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id != currentUserId && u.Username.ToLower().Contains(normalized))
            .Take(limit)
            .Select(u => u.Username)
            .ToListAsync(ct);
    }
    
    public async Task<ReceiveUserStatusDto?> GetUserStatusAsync(int userId, CancellationToken ct)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new ReceiveUserStatusDto
            {
                Id = u.Id,
                Status = u.Status
            })
            .FirstOrDefaultAsync(ct);
    }
    public async Task<bool> ExistsAsync(string email, string username, CancellationToken ct)
    {
        var normalizedEmail = email.ToLower().Trim();
        var normalizedUsername = username.ToLower().Trim();

        return await db.Users.AnyAsync(u =>
            u.Email.ToLower() == normalizedEmail ||
            u.Username.ToLower() == normalizedUsername, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await db.Users.AddAsync(user, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
    
    public async Task<User?> GetByIdentifierAsync(string identifier, CancellationToken ct)
    {
        var normalized = identifier.ToLower().Trim();
        
        return await db.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == normalized || u.Username.ToLower() == normalized, ct);
    }
}