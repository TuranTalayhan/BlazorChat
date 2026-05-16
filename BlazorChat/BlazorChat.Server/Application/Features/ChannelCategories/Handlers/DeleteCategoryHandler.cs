using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Server.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Handlers;

public class DeleteCategoryHandler(AppDbContext db) : ICommandHandler<DeleteCategoryCommand, CategoryResult>
{
    public async ValueTask<CategoryResult> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await db.ChannelCategories
            .Include(c => c.Server)
            .Include(c => c.Channels)
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);

        if (category == null)
            return CategoryResult.Failure(CategoryError.NotFound);

        if (category.Server.OwnerId != request.UserId)
            return CategoryResult.Failure(CategoryError.Forbidden);

        foreach (var channel in category.Channels)
        {
            channel.CategoryId = null;
        }

        db.ChannelCategories.Remove(category);
        await db.SaveChangesAsync(ct);
        
        return CategoryResult.Success();
    }
}