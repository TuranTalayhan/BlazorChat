using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerChannelsQueryHandler(IServerRepository serverRepository) 
    : IQueryHandler<GetServerChannelsQuery, ServerResult<List<ChannelDto>>>
{
    public async ValueTask<ServerResult<List<ChannelDto>>> Handle(GetServerChannelsQuery request, CancellationToken ct)
    {
        var isMember = await serverRepository.IsMemberAsync(request.ServerId, request.CurrentUserId, ct);
        if (!isMember) 
        {
            return new ServerResult<List<ChannelDto>>(false, Error: ServerError.Forbidden);
        }

        var channels = await serverRepository.GetChannelsByServerIdAsync(request.ServerId, ct);

        return new ServerResult<List<ChannelDto>>(true, Data: channels);
    }
}