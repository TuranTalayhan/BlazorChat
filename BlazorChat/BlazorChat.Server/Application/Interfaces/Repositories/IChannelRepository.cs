using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IChannelRepository
{
    Task<List<ChannelDto>> GetUserDirectMessagesAsync(int userId, CancellationToken ct);
    Task<ChannelDto?> GetChannelWithDetailsAsync(int channelId, CancellationToken ct);
    Task<bool> IsServerMemberAsync(int serverId, int userId, CancellationToken ct);
    Task<Channel?> GetDirectMessageByMembersAsync(int currentUserId, int friendId, CancellationToken ct);
    Task<int?> GetDirectMessageIdByMembersAsync(int currentUserId, int friendId, CancellationToken ct);
    Task<(User? Current, User? Friend)> GetDmUsersAsync(int currentUserId, int friendId, CancellationToken ct);    Task<bool> UsersExistAsync(int userId1, int userId2, CancellationToken ct);
    Task AddAsync(Channel channel, CancellationToken ct);
    Task<Channel?> GetByIdAsync(int channelId, CancellationToken ct);
    void Remove(Channel channel);
    Task SaveChangesAsync(CancellationToken ct);
}