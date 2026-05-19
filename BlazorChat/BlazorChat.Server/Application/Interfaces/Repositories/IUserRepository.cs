using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(string email, string username, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<User?> GetByIdentifierAsync(string identifier, CancellationToken ct);
}