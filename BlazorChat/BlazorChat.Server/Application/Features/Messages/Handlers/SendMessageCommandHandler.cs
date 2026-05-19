using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class SendMessageCommandHandler(
    IMessageRepository messageRepository, 
    IChannelAuthorizationService authService, 
    IChatNotificationService notifications) 
    : ICommandHandler<SendMessageCommand, MessageResult<MessageDto>>
{
    public async ValueTask<MessageResult<MessageDto>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        if (request.Dto.ChannelId <= 0)
            return new MessageResult<MessageDto>(false, Error: MessageError.BadRequest, ErrorMessage: "Invalid channel ID.");

        var canAccess = await authService.CanAccessChannelAsync(request.CurrentUserId, request.Dto.ChannelId, ct);
        if (!canAccess)
            return new MessageResult<MessageDto>(false, Error: MessageError.Forbidden, ErrorMessage: "Access denied.");

        var user = await messageRepository.GetUserByIdAsync(request.CurrentUserId, ct);
        if (user == null)
            return new MessageResult<MessageDto>(false, Error: MessageError.NotFound, ErrorMessage: "User not found.");

        var message = Message.Create(request.Dto.Content, request.Dto.ChannelId, request.CurrentUserId);
        
        await messageRepository.AddAsync(message, ct);
        await messageRepository.SaveChangesAsync(ct);

        var messageDto = new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            AuthorId = message.AuthorId,
            AuthorUsername = user.Username,
            AuthorAvatarUrl = user.AvatarUrl,
            ChannelId = message.ChannelId
        };

        var recipientUserId = await messageRepository.GetDmRecipientIdAsync(message.ChannelId, message.AuthorId, ct);
        
        await notifications.SendMessageToChannelAsync(message.ChannelId, recipientUserId, messageDto);

        return new MessageResult<MessageDto>(true, Data: messageDto);
    }
}