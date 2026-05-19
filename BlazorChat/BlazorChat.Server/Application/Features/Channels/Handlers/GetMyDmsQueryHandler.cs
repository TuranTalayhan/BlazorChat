using BlazorChat.Server.Application.Features.Channels.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class GetMyDmsQueryHandler(IChannelRepository channelRepository) 
    : IQueryHandler<GetMyDmsQuery, List<ChannelDto>>
{
    public async ValueTask<List<ChannelDto>> Handle(GetMyDmsQuery request, CancellationToken ct)
    {
        return await channelRepository.GetUserDirectMessagesAsync(request.CurrentUserId, ct);
    }
}