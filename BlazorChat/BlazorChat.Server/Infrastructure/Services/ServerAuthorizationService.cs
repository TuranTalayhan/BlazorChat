using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Server.Infrastructure.Services;

public class ServerAuthorizationService(IServerRepository serverRepository) : IServerAuthorizationService
{
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