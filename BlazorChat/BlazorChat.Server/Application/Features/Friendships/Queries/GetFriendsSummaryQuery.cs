using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Friendships.Queries;

public record GetFriendsSummaryQuery(int UserId) : IQuery<List<SidebarFriendSummaryDto>>;