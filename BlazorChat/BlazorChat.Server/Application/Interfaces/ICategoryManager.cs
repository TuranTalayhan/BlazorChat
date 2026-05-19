using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Server.Application.Interfaces;

public interface ICategoryManager
{
    Task<ChannelCategory?> ResolveCategoryAsync(int serverId, int? categoryId, string? categoryName, CancellationToken ct = default);
}