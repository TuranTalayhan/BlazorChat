using BlazorChat.Server.Application.Features.Friendships.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;


public class GetPendingRequestsQueryHandler(IFriendshipRepository friendshipRepository) 
    : IQueryHandler<GetPendingRequestsQuery, List<PendingFriendshipDto>>
{
    public async ValueTask<List<PendingFriendshipDto>> Handle(GetPendingRequestsQuery request, CancellationToken ct)
    {
        return await friendshipRepository.GetPendingRequestsAsync(request.CurrentUserId, ct);
    }
}
