using BlazorChat.Server.Application.Features.Messages.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class GetUnreadStatusesQueryHandler(IMessageRepository messageRepository) 
    : IQueryHandler<GetUnreadStatusesQuery, List<ChannelUnreadStatusDto>>
{
    public async ValueTask<List<ChannelUnreadStatusDto>> Handle(GetUnreadStatusesQuery request, CancellationToken ct)
    {
        return await messageRepository.GetUnreadStatusesForUserAsync(request.UserId, ct);
    }
}