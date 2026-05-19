using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<ChannelCategory?> GetByIdAsync(int categoryId, CancellationToken ct);
    void Remove(ChannelCategory category);
    Task<ChannelCategory?> GetByIdWithChannelsAsync(int categoryId, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int serverId, string name, CancellationToken ct);
    Task<int> GetNextSortOrderAsync(int serverId, CancellationToken ct);
    Task AddAsync(ChannelCategory category, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}