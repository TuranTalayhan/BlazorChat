using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerCategoriesQueryHandler(AppDbContext db) 
    : IQueryHandler<GetServerCategoriesQuery, ServerResult<List<CategoryDto>>>
{
    public async ValueTask<ServerResult<List<CategoryDto>>> Handle(GetServerCategoriesQuery request, CancellationToken ct)
    {
        var isMember = await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == request.ServerId && sm.UserId == request.CurrentUserId, ct);
            
        if (!isMember) 
            return new ServerResult<List<CategoryDto>>(false, Error: ServerError.Forbidden);

        var categories = await db.ChannelCategories
            .AsNoTracking()
            .Where(c => c.ServerId == request.ServerId)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SortOrder = c.SortOrder
            })
            .ToListAsync(ct);

        return new ServerResult<List<CategoryDto>>(true, Data: categories);
    }
}