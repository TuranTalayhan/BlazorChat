using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Server.Infrastructure.Services;

public class CategoryManager(ICategoryRepository categoryRepository) : ICategoryManager
{
    public async Task<ChannelCategory?> ResolveCategoryAsync(int serverId, int? categoryId, string? categoryName, CancellationToken ct = default)
    {
        if (categoryId is > 0)
        {
            return await categoryRepository.GetByIdAndServerAsync(categoryId.Value, serverId, ct);
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return null;
        }

        var trimmedName = categoryName.Trim();
        
        var existingCategory = await categoryRepository.GetByNameAndServerAsync(trimmedName, serverId, ct);
        if (existingCategory != null)
        {
            return existingCategory;
        }

        var newCategory = ChannelCategory.Create(trimmedName, serverId, sortOrder: 0);

        await categoryRepository.AddAsync(newCategory, ct);
        await categoryRepository.SaveChangesAsync(ct);

        return newCategory;
    }
}