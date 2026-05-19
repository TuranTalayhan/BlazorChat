using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Handlers;

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IServerAuthorizationService authService) 
    : ICommandHandler<CreateCategoryCommand, ServerResult<CategoryDto>>
{
    public async ValueTask<ServerResult<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var isAuthorized = await authService.IsAdminOrOwnerAsync(request.ServerId, request.CurrentUserId, ct);
        if (!isAuthorized)
        {
            return new ServerResult<CategoryDto>(false, Error: ServerError.Forbidden);
        }

        var categoryExists = await categoryRepository.ExistsByNameAsync(request.ServerId, request.Name, ct);
        if (categoryExists)
        {
            return new ServerResult<CategoryDto>(false, Error: ServerError.BadRequest, ErrorMessage: "A category with this name already exists.");
        }

        var nextSortOrder = await categoryRepository.GetNextSortOrderAsync(request.ServerId, ct);

        var category = ChannelCategory.Create(request.Name, request.ServerId, nextSortOrder);

        await categoryRepository.AddAsync(category, ct);
        await categoryRepository.SaveChangesAsync(ct);

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            SortOrder = category.SortOrder
        };

        return new ServerResult<CategoryDto>(true, Data: dto);
    }
}