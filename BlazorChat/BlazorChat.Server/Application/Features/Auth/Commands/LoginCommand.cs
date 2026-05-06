using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Auth.Commands;

public record LoginCommand(LoginDto Dto) : ICommand<AuthResult>;