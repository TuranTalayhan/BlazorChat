using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Users.Commands;

public record UpdateUserStatusCommand(int CurrentUserId, UpdateStatusDto Dto) : ICommand<bool>;