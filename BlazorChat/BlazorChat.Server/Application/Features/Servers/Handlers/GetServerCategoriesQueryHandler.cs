using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerCategoriesQueryHandler(IServerRepository serverRepository) 
    : IQueryHandler<GetServerCategoriesQuery, ServerResult<List<CategoryDto>>>
{
    public async ValueTask<ServerResult<List<CategoryDto>>> Handle(GetServerCategoriesQuery request, CancellationToken ct)
    {
        var isMember = await serverRepository.IsMemberAsync(request.ServerId, request.CurrentUserId, ct);
        if (!isMember) 
        {
            return new ServerResult<List<CategoryDto>>(false, Error: ServerError.Forbidden);
        }

        var categories = await serverRepository.GetCategoriesByServerIdAsync(request.ServerId, ct);

        return new ServerResult<List<CategoryDto>>(true, Data: categories);
    }
}