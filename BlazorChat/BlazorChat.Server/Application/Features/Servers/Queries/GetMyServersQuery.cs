using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Queries;

public record GetMyServersQuery(int CurrentUserId) : IQuery<List<ServerDto>>;