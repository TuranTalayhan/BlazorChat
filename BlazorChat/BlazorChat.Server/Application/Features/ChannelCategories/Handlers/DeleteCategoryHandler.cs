using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using Mediator;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Handlers;

public class DeleteCategoryHandler(
    ICategoryRepository categoryRepository,
    IServerAuthorizationService authService) 
    : ICommandHandler<DeleteCategoryCommand, CategoryResult>
{
    public async ValueTask<CategoryResult> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdWithChannelsAsync(request.CategoryId, ct);
        if (category == null)
        {
            return CategoryResult.Failure(CategoryError.NotFound);
        }

        var isAuthorized = await authService.IsAdminOrOwnerAsync(category.ServerId, request.UserId, ct);
        if (!isAuthorized)
        {
            return CategoryResult.Failure(CategoryError.Forbidden);
        }

        category.PrepareForDeletion();

        categoryRepository.Remove(category);
        await categoryRepository.SaveChangesAsync(ct);
        
        return CategoryResult.Success();
    }
}