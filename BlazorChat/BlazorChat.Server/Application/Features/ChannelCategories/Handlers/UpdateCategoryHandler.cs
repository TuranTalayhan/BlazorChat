using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using Mediator;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Handlers;

public class UpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IServerAuthorizationService authService) 
    : ICommandHandler<UpdateCategoryCommand, CategoryResult>
{
    public async ValueTask<CategoryResult> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct);
        if (category == null)
        {
            return CategoryResult.Failure(CategoryError.NotFound, "Category not found.");
        }

        var isAuthorized = await authService.IsAdminOrOwnerAsync(category.ServerId, request.UserId, ct);
        if (!isAuthorized)
        {
            return CategoryResult.Failure(CategoryError.Forbidden, "You do not have permission.");
        }

        category.Rename(request.Dto.Name);

        await categoryRepository.SaveChangesAsync(ct);
        
        return CategoryResult.Success();
    }
}