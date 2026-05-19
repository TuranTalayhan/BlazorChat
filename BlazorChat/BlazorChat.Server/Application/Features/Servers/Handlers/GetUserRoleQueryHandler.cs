using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.Enums;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetUserRoleQueryHandler(IServerRepository serverRepository) 
    : IQueryHandler<GetUserRoleQuery, ServerRole>
{
    public async ValueTask<ServerRole> Handle(GetUserRoleQuery request, CancellationToken ct)
    {
        var role = await serverRepository.GetUserRoleInServerAsync(request.ServerId, request.UserId, ct);
        
        return role ?? ServerRole.Member;
    }
}