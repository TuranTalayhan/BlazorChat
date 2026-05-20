using BlazorChat.Shared.Enums;

namespace BlazorChat.Server.Application.Interfaces;

public interface IServerAuthorizationService
{
    Task<bool> IsAdminOrOwnerAsync(int? serverId, int userId, CancellationToken ct);
    
    Task<ServerRole?> GetUserRoleInServerAsync(int serverId, int userId, CancellationToken ct);
}