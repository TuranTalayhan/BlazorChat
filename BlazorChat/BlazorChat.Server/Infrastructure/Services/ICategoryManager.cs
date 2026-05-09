using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Services;

public interface ICategoryManager
{
    Task<ChannelCategory?> ResolveCategoryAsync(int serverId, int? categoryId, string? categoryName, CancellationToken ct = default);
}

public class CategoryManager(AppDbContext db) : ICategoryManager
{
    public async Task<ChannelCategory?> ResolveCategoryAsync(int serverId, int? categoryId, string? categoryName, CancellationToken ct = default)
    {
        // SCENARIO A: Explicit Category ID provided
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            // Attempt to find it. If they provided a fake/malicious ID, 
            // this safely returns null.
            return await db.ChannelCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId.Value && c.ServerId == serverId, ct);
        }

        // SCENARIO B: A Category Name was provided (Get or Create)
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var normalizedName = categoryName.Trim();

            var category = await db.ChannelCategories
                .FirstOrDefaultAsync(c => c.ServerId == serverId 
                                       && c.Name.ToLower() == normalizedName.ToLower(), ct);

            if (category == null)
            {
                category = new ChannelCategory
                {
                    Name = normalizedName,
                    ServerId = serverId,
                    SortOrder = 0 // Standard default, pushes it to the top
                };

                db.ChannelCategories.Add(category);
                
                // We MUST save here so EF Core generates the .Id property 
                // for the channel creation step that happens later.
                await db.SaveChangesAsync(ct);
            }

            return category;
        }

        // SCENARIO C: No Category requested (uncategorized channel)
        return null;
    }
}