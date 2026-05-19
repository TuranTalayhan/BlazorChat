using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IServerRepository
{
    Task AddInviteAsync(ServerInvite invite, CancellationToken ct);
    Task<ServerInvite?> GetInviteWithServerByCodeAsync(string code, CancellationToken ct);
    Task AddMembershipAsync(ServerMembership membership, CancellationToken ct);
    Task<ServerRole?> GetUserRoleInServerAsync(int serverId, int userId, CancellationToken ct);
    Task<List<ChannelDto>> GetChannelsByServerIdAsync(int serverId, CancellationToken ct);
    Task<bool> IsMemberAsync(int serverId, int userId, CancellationToken ct);
    Task<List<CategoryDto>> GetCategoriesByServerIdAsync(int serverId, CancellationToken ct);
    Task<(ServerLookupStatus Status, ServerDto? Data)> GetServerForUserAsync(int serverId, int userId, CancellationToken ct);
    Task AddAsync(ChatServer server, CancellationToken ct);
    Task<List<ServerDto>> GetUserJoinedServersAsync(int userId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}