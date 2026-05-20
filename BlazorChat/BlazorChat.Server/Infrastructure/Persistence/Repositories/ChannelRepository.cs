using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class ChannelRepository(AppDbContext db) : IChannelRepository
{
    
    public async Task<ServerDto?> GetServerByChannelIdAsync(int channelId, CancellationToken ct)
    {
        return await db.Channels
            .AsNoTracking()
            .Where(c => c.Id == channelId && c.Server != null)
            .Select(c => new ServerDto
            {
                Id = c.Server!.Id,
                Name = c.Server.Name
            })
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<List<ChannelDto>> GetUserDirectMessagesAsync(int userId, CancellationToken ct)
    {
        return await db.Channels
            .AsNoTracking()
            .Where(c => c.Type == ChannelType.DirectMessage && c.Members.Any(m => m.Id == userId))
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ChannelDto
            {
                Id = c.Id,
                Type = c.Type,
                Members = c.Members.Select(m => new UserDto
                {
                    Id = m.Id,
                    Username = m.Username,
                    AvatarUrl = m.AvatarUrl ?? "" // Handle null fallback safely
                }).ToList()
            })
            .ToListAsync(ct);
    }
    
    public async Task<ChannelDto?> GetChannelWithDetailsAsync(int channelId, CancellationToken ct)
    {
        return await db.Channels
            .AsNoTracking()
            .Where(c => c.Id == channelId)
            .Select(channel => new ChannelDto
            {
                Id = channel.Id,
                Name = channel.Name,
                ServerId = channel.ServerId,
                SortOrder = channel.SortOrder,
                Type = channel.Type,
                CreatedAt = channel.CreatedAt,
                UpdatedAt = channel.UpdatedAt,
                
                Members = channel.Members.Select(m => new UserDto
                {
                    Id = m.Id,
                    Username = m.Username,
                    AvatarUrl = m.AvatarUrl
                }).ToList(),

                Category = channel.Category != null ? new CategoryDto 
                { 
                    Id = channel.Category.Id, 
                    Name = channel.Category.Name 
                } : null
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsServerMemberAsync(int serverId, int userId, CancellationToken ct)
    {
        return await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId, ct);
    }
    
    public async Task<int?> GetDirectMessageIdByMembersAsync(int currentUserId, int friendId, CancellationToken ct)
    {
        var id = await db.Channels
            .Where(c => c.Type == ChannelType.DirectMessage)
            .Where(c => c.Members.Any(m => m.Id == currentUserId) && 
                        c.Members.Any(m => m.Id == friendId))
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);

        return id;
    }

    public async Task<(User? Current, User? Friend)> GetDmUsersAsync(int currentUserId, int friendId, CancellationToken ct)
    {
        var users = await db.Users
            .Where(u => u.Id == currentUserId || u.Id == friendId)
            .ToListAsync(ct);

        return (
            Current: users.FirstOrDefault(u => u.Id == currentUserId),
            Friend: users.FirstOrDefault(u => u.Id == friendId)
        );
    }
    
    public async Task<Channel?> GetDirectMessageByMembersAsync(int currentUserId, int friendId, CancellationToken ct)
    {
        return await db.Channels
            .Where(c => c.Type == ChannelType.DirectMessage)
            .FirstOrDefaultAsync(c => 
                c.Members.Any(m => m.Id == currentUserId) && 
                c.Members.Any(m => m.Id == friendId), ct);
    }

    public async Task<bool> UsersExistAsync(int userId1, int userId2, CancellationToken ct)
    {
        var count = await db.Users.CountAsync(u => u.Id == userId1 || u.Id == userId2, ct);
        return count == 2;
    }

    public async Task AddAsync(Channel channel, CancellationToken ct)
    {
        await db.Channels.AddAsync(channel, ct);
    }

    public async Task<Channel?> GetByIdAsync(int channelId, CancellationToken ct)
    {
        return await db.Channels.FirstOrDefaultAsync(c => c.Id == channelId, ct);
    }

    public void Remove(Channel channel)
    {
        db.Channels.Remove(channel);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}