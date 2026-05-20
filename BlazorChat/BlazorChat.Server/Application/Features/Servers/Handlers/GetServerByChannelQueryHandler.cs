using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerByChannelQueryHandler(IChannelRepository repository) 
    : IQueryHandler<GetServerByChannelQuery, ServerDto?>
{
    public async ValueTask<ServerDto?> Handle(GetServerByChannelQuery request, CancellationToken ct)
    {
        return await repository.GetServerByChannelIdAsync(request.ChannelId, ct);
    }
}