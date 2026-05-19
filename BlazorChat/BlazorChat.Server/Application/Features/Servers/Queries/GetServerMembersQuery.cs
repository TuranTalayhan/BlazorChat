using BlazorChat.Server.Application.Common;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Queries;

public record GetServerMembersQuery(int ServerId, int CurrentUserId) : IQuery<Result<List<UserDto>>>;