using BlazorChat.Shared.Enums;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Queries;

public record GetUserRoleQuery(int ServerId, int UserId) : IQuery<ServerRole>;