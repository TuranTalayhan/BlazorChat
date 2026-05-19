using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<ChannelCategory?> GetByIdAsync(int categoryId, CancellationToken ct)
    {
        return await db.ChannelCategories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
    }
    public async Task<ChannelCategory?> GetByIdWithChannelsAsync(int categoryId, CancellationToken ct)
    {
        return await db.ChannelCategories
            .Include(c => c.Channels)
            .FirstOrDefaultAsync(c => c.Id == categoryId, ct);
    }

    public void Remove(ChannelCategory category)
    {
        db.ChannelCategories.Remove(category);
    }
    
    public async Task<bool> ExistsByNameAsync(int serverId, string name, CancellationToken ct)
    {
        var normalized = name.Trim().ToLower();
        return await db.ChannelCategories
            .AnyAsync(c => c.ServerId == serverId && c.Name.ToLower() == normalized, ct);
    }

    public async Task<int> GetNextSortOrderAsync(int serverId, CancellationToken ct)
    {
        var maxSortOrder = await db.ChannelCategories
            .Where(c => c.ServerId == serverId)
            .MaxAsync(c => (int?)c.SortOrder, ct) ?? -1;

        return maxSortOrder + 1;
    }

    public async Task AddAsync(ChannelCategory category, CancellationToken ct)
    {
        await db.ChannelCategories.AddAsync(category, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}