using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId, CancellationToken ct);
    Task<List<string>> GetAcceptedFriendIdsAsync(int userId, CancellationToken ct);
    Task<List<string>> SearchUsernamesAsync(int currentUserId, string searchTerm, int limit, CancellationToken ct);
    Task<ReceiveUserStatusDto?> GetUserStatusAsync(int userId, CancellationToken ct);
    Task<bool> ExistsAsync(string email, string username, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<User?> GetByIdentifierAsync(string identifier, CancellationToken ct);
}