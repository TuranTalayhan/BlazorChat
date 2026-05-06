using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Commands;

public record SendMessageCommand(int CurrentUserId, SendMessageDto Dto) : ICommand<MessageResult<MessageDto>>;