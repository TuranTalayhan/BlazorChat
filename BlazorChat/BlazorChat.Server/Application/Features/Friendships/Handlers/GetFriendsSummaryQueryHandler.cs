using BlazorChat.Server.Application.Features.Friendships.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class GetFriendsSummaryQueryHandler(IFriendshipRepository friendshipRepository) 
    : IQueryHandler<GetFriendsSummaryQuery, List<SidebarFriendSummaryDto>>
{
    public async ValueTask<List<SidebarFriendSummaryDto>> Handle(GetFriendsSummaryQuery request, CancellationToken ct)
    {
        return await friendshipRepository.GetFriendsSummaryAsync(request.UserId, ct);
    }
}