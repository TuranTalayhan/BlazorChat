using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Queries;

public record GetFriendsQuery(int CurrentUserId) : IQuery<List<FriendshipDto>>;