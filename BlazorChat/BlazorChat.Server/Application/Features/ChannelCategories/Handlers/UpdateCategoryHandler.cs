using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Server.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Handlers;

public class UpdateCategoryHandler(AppDbContext db) : ICommandHandler<UpdateCategoryCommand, CategoryResult>
{
    public async ValueTask<CategoryResult> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await db.ChannelCategories
            .Include(c => c.Server)
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);

        if (category == null)
            return CategoryResult.Failure(CategoryError.NotFound, "Category not found.");

        if (category.Server.OwnerId != request.UserId)
            return CategoryResult.Failure(CategoryError.Forbidden, "You do not have permission.");

        category.Name = request.Dto.Name;
        
        await db.SaveChangesAsync(ct);
        return CategoryResult.Success();
    }
}