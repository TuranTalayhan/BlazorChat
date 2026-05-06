using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Queries;

public record GetPendingRequestsQuery(int CurrentUserId) : IQuery<List<PendingFriendshipDto>>;