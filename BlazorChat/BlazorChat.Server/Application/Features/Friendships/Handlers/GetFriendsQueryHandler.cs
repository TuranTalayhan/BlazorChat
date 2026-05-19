using BlazorChat.Server.Application.Features.Friendships.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Handlers;

public class GetFriendsQueryHandler(IFriendshipRepository friendshipRepository) 
    : IQueryHandler<GetFriendsQuery, List<FriendshipDto>>
{
    public async ValueTask<List<FriendshipDto>> Handle(GetFriendsQuery request, CancellationToken ct)
    {
        return await friendshipRepository.GetAcceptedFriendsAsync(request.CurrentUserId, ct);
    }
}