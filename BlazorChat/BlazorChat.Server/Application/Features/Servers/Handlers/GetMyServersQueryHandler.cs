using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetMyServersQueryHandler(IServerRepository serverRepository) 
    : IQueryHandler<GetMyServersQuery, List<ServerDto>>
{
    public async ValueTask<List<ServerDto>> Handle(GetMyServersQuery request, CancellationToken ct)
    {
        return await serverRepository.GetUserJoinedServersAsync(request.CurrentUserId, ct);
    }
}