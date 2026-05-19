using BlazorChat.Server.Application.Common;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Commands;

public record CreateInviteCommand(int ServerId, int UserId, CreateInviteDto Dto) : ICommand<Result<InviteResponseDto>>;