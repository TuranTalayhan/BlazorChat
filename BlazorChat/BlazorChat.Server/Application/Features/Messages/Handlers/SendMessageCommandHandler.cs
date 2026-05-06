using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class SendMessageCommandHandler(AppDbContext db, IChannelAuthorizationService authService, IChatNotificationService notifications) 
    : ICommandHandler<SendMessageCommand, MessageResult<MessageDto>>
{
    public async ValueTask<MessageResult<MessageDto>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        if (request.Dto.ChannelId <= 0)
            return new MessageResult<MessageDto>(false, Error: MessageError.BadRequest, ErrorMessage: "Invalid channel ID.");

        if (!await authService.CanAccessChannelAsync(request.CurrentUserId, request.Dto.ChannelId, ct))
            return new MessageResult<MessageDto>(false, Error: MessageError.Forbidden, ErrorMessage: "Access denied.");

        var user = await db.Users.FindAsync([request.CurrentUserId], ct);
        if (user == null)
            return new MessageResult<MessageDto>(false, Error: MessageError.NotFound, ErrorMessage: "User not found.");

        var message = new Message
        {
            Content = request.Dto.Content.Trim(),
            ChannelId = request.Dto.ChannelId,
            AuthorId = request.CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);

        var messageDto = new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            AuthorId = request.CurrentUserId,
            AuthorUsername = user.Username,
            AuthorAvatarUrl = user.AvatarUrl,
            ChannelId = message.ChannelId
        };

        await notifications.SendMessageToChannelAsync(request.Dto.ChannelId, messageDto);

        return new MessageResult<MessageDto>(true, Data: messageDto);
    }
}