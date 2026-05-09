using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Client.Features.Channels;

public record GetChannelsQuery(int ServerId) : IQuery<List<ChannelDto>>;

public class ChannelHandlers(IChannelApiService api) : IQueryHandler<GetChannelsQuery, List<ChannelDto>>
{
    public async ValueTask<List<ChannelDto>> Handle(GetChannelsQuery request, CancellationToken ct) =>
        await api.GetChannelsGetByServerAsync(request.ServerId, ct);
}