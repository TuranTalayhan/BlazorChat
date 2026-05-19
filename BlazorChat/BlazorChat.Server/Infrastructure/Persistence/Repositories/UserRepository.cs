using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<bool> ExistsAsync(string email, string username, CancellationToken ct)
    {
        var normalizedEmail = email.ToLower().Trim();
        var normalizedUsername = username.ToLower().Trim();

        return await db.Users.AnyAsync(u =>
            u.Email.ToLower() == normalizedEmail ||
            u.Username.ToLower() == normalizedUsername, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await db.Users.AddAsync(user, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
    
    public async Task<User?> GetByIdentifierAsync(string identifier, CancellationToken ct)
    {
        var normalized = identifier.ToLower().Trim();
        
        return await db.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == normalized || u.Username.ToLower() == normalized, ct);
    }
}