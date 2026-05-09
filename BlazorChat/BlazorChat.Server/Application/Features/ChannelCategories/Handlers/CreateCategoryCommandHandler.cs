using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Handlers;

public class CreateCategoryCommandHandler(AppDbContext db) 
    : ICommandHandler<CreateCategoryCommand, ServerResult<CategoryDto>>
{
    public async ValueTask<ServerResult<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var membership = await db.ServerMemberships
            .FirstOrDefaultAsync(sm => sm.ServerId == request.ServerId && sm.UserId == request.CurrentUserId, ct);

        if (membership == null || membership.Role == ServerRole.Member)
            return new ServerResult<CategoryDto>(false, Error: ServerError.Forbidden);

        var normalizedName = request.Name.Trim();

        var categoryExists = await db.ChannelCategories
            .AnyAsync(c => c.ServerId == request.ServerId && c.Name.ToLower() == normalizedName.ToLower(), ct);

        if (categoryExists)
        {
            return new ServerResult<CategoryDto>(false, Error: ServerError.BadRequest, ErrorMessage: "A category with this name already exists.");
        }

        var maxSortOrder = await db.ChannelCategories
            .Where(c => c.ServerId == request.ServerId)
            .MaxAsync(c => (int?)c.SortOrder, ct) ?? -1;

        var category = new ChannelCategory
        {
            Name = normalizedName,
            ServerId = request.ServerId,
            SortOrder = maxSortOrder + 1
        };

        db.ChannelCategories.Add(category);
        await db.SaveChangesAsync(ct);

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            SortOrder = category.SortOrder
        };

        return new ServerResult<CategoryDto>(true, Data: dto);
    }
}