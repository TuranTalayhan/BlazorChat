using BlazorChat.Server.Application.Common;
using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerMembersQueryHandler(IServerRepository serverRepository) 
    : IQueryHandler<GetServerMembersQuery, Result<List<UserDto>>>
{
    public async ValueTask<Result<List<UserDto>>> Handle(GetServerMembersQuery request, CancellationToken ct)
    {
        var isMember = await serverRepository.IsMemberAsync(request.ServerId, request.CurrentUserId, ct);
        if (!isMember)
        {
            return Result<List<UserDto>>.Failure("Access Denied.", ErrorType.Forbidden);
        }

        var members = await serverRepository.GetMembersByServerIdAsync(request.ServerId, ct);

        return Result<List<UserDto>>.Success(members);
    }
}