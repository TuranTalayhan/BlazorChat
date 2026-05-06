using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Queries;

public record GetServerByIdQuery(int CurrentUserId, int ServerId) : IQuery<ServerResult<ServerDto>>;