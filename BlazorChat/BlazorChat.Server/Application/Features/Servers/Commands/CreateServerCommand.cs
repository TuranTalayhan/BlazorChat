using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Commands;

public record CreateServerCommand(int CurrentUserId, CreateServerDto Dto) : ICommand<ServerDto>;