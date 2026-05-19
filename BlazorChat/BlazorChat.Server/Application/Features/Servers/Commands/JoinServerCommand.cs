using BlazorChat.Server.Application.Common;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Commands;

public record JoinServerCommand(string Code, int UserId) : ICommand<Result<ServerDto>>;