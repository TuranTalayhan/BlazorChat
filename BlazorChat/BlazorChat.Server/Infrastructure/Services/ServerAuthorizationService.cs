using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.Enums;

namespace BlazorChat.Server.Infrastructure.Services;

public class ServerAuthorizationService(IServerRepository serverRepository) : IServerAuthorizationService
{
    public async Task<ServerRole?> GetUserRoleInServerAsync(int serverId, int userId, CancellationToken ct)
    {
        if (serverId <= 0 || userId <= 0)
        {
            return ServerRole.Member;
        }

        return await serverRepository.GetUserRoleInServerAsync(serverId, userId, ct);
    }
    
    public async Task<bool> IsAdminOrOwnerAsync(int? serverId, int userId, CancellationToken ct)
    {
        if (serverId is not > 0)
        {
            return false;
        }

        var userRole = await serverRepository.GetUserRoleInServerAsync(serverId.Value, userId, ct);

        return userRole is ServerRole.Admin or ServerRole.Owner;
    }
}