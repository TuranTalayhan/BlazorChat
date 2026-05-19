using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerByIdQueryHandler(IServerRepository serverRepository) 
    : IQueryHandler<GetServerByIdQuery, ServerResult<ServerDto>>
{
    public async ValueTask<ServerResult<ServerDto>> Handle(GetServerByIdQuery request, CancellationToken ct)
    {
        var (status, dto) = await serverRepository.GetServerForUserAsync(request.ServerId, request.CurrentUserId, ct);

        return status switch
        {
            ServerLookupStatus.Success   => new ServerResult<ServerDto>(true, Data: dto),
            ServerLookupStatus.Forbidden => new ServerResult<ServerDto>(false, Error: ServerError.Forbidden),
            _                            => new ServerResult<ServerDto>(false, Error: ServerError.NotFound)
        };
    }
}