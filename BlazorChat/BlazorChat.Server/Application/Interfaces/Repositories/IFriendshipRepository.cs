using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IFriendshipRepository
{
    Task AddAsync(Friendship friendship, CancellationToken ct);
    Task<bool> FriendshipExistsAsync(int userId1, int userId2, CancellationToken ct);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct);
    Task<string?> GetUsernameByIdAsync(int userId, CancellationToken ct);
    Task<Friendship?> GetPendingRequestAsync(int requesterId, int receiverId, CancellationToken ct);
    Task<User?> GetUserByIdAsync(int userId, CancellationToken ct);
    void Remove(Friendship friendship);
    Task SaveChangesAsync(CancellationToken ct);
    Task<List<PendingFriendshipDto>> GetPendingRequestsAsync(int userId, CancellationToken ct);
    Task<List<SidebarFriendSummaryDto>> GetFriendsSummaryAsync(int userId, CancellationToken ct);
    Task<List<FriendshipDto>> GetAcceptedFriendsAsync(int userId, CancellationToken ct);
}