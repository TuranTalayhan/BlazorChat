using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Services;

public class ServerAuthorizationService(AppDbContext db) : IServerAuthorizationService
{
    public async Task<bool> IsAdminOrOwnerAsync(int? serverId, int userId, CancellationToken ct)
    {
        return await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == serverId 
                            && sm.UserId == userId 
                            && sm.Role != ServerRole.Member, ct);
    }
}