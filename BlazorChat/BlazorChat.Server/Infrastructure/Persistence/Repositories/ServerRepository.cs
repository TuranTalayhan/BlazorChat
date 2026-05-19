using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class ServerRepository(AppDbContext db) : IServerRepository
{
    public async Task<List<UserDto>> GetMembersByServerIdAsync(int serverId, CancellationToken ct)
    {
        return await db.ServerMemberships
            .AsNoTracking()
            .Where(sm => sm.ServerId == serverId)
            .OrderByDescending(sm => sm.Role)
            .ThenBy(sm => sm.User.Username) 
            .Select(sm => new UserDto
            {
                Id = sm.User.Id,
                Username = sm.User.Username,
                AvatarUrl = sm.User.AvatarUrl,
                Status = sm.User.Status,
                Role = sm.Role
            })
            .ToListAsync(ct);
    }
    
    public async Task<ServerRole?> GetUserRoleInServerAsync(int serverId, int userId, CancellationToken ct)
    {
        return await db.ServerMemberships
            .Where(sm => sm.ServerId == serverId && sm.UserId == userId)
            .Select(sm => (ServerRole?)sm.Role)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddInviteAsync(ServerInvite invite, CancellationToken ct)
    {
        await db.Set<ServerInvite>().AddAsync(invite, ct);
    }

    public async Task<ServerInvite?> GetInviteWithServerByCodeAsync(string code, CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpper();
        return await db.Set<ServerInvite>()
            .Include(si => si.Server)
            .FirstOrDefaultAsync(si => si.Code == normalizedCode, ct);
    }

    public async Task AddMembershipAsync(ServerMembership membership, CancellationToken ct)
    {
        await db.Set<ServerMembership>().AddAsync(membership, ct);
    }
    
    public async Task<List<ChannelDto>> GetChannelsByServerIdAsync(int serverId, CancellationToken ct)
    {
        return await db.Channels
            .AsNoTracking()
            .Where(c => c.ServerId == serverId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.CreatedAt)
            .Select(c => new ChannelDto 
            { 
                Id = c.Id, 
                Name = c.Name, 
                Type = c.Type,
                ServerId = c.ServerId, 
                SortOrder = c.SortOrder,
                Category = c.Category != null ? new CategoryDto
                {
                    Id = c.Category.Id,
                    Name = c.Category.Name,
                    SortOrder = c.Category.SortOrder
                } : null
            })
            .ToListAsync(ct);
    }
    public async Task<bool> IsMemberAsync(int serverId, int userId, CancellationToken ct)
    {
        return await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId, ct);
    }

    public async Task<List<CategoryDto>> GetCategoriesByServerIdAsync(int serverId, CancellationToken ct)
    {
        return await db.ChannelCategories
            .AsNoTracking()
            .Where(c => c.ServerId == serverId)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SortOrder = c.SortOrder
            })
            .ToListAsync(ct);
    }
    public async Task<(ServerLookupStatus Status, ServerDto? Data)> GetServerForUserAsync(int serverId, int userId, CancellationToken ct)
    {
        var serverDto = await db.ServerMemberships
            .AsNoTracking()
            .Where(sm => sm.ServerId == serverId && sm.UserId == userId)
            .Select(sm => new ServerDto
            {
                Id = sm.Server.Id,
                Name = sm.Server.Name,
                OwnerId = sm.Server.OwnerId
            })
            .FirstOrDefaultAsync(ct);

        if (serverDto != null)
        {
            return (ServerLookupStatus.Success, serverDto);
        }

        var serverExists = await db.Servers.AnyAsync(s => s.Id == serverId, ct);
        
        var failureStatus = serverExists 
            ? ServerLookupStatus.Forbidden 
            : ServerLookupStatus.NotFound;

        return (failureStatus, null);
    }
    public async Task<List<ServerDto>> GetUserJoinedServersAsync(int userId, CancellationToken ct)
    {
        return await db.ServerMemberships
            .AsNoTracking()
            .Where(sm => sm.UserId == userId)
            .Select(sm => new ServerDto
            {
                Id = sm.Server.Id,
                Name = sm.Server.Name,
                OwnerId = sm.Server.OwnerId
            })
            .ToListAsync(ct);
    }
    public async Task AddAsync(ChatServer server, CancellationToken ct)
    {
        await db.Servers.AddAsync(server, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}