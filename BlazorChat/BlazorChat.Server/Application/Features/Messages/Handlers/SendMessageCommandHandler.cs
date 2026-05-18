using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

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

        // 1. Save the new message to the database
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

        // 2. DYNAMICALLY FIND THE RECIPIENT
        // Look up the channel members to see who the OTHER person is
        var recipientUserId = await db.Channels
            .Where(c => c.Id == request.Dto.ChannelId)
            .SelectMany(c => c.Members)
            .Where(m => m.Id != request.CurrentUserId) // Exclude the sender
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);

        // 3. Dispatch the notification out through your updated service
        // (recipientUserId will be 0 if it's a server channel with no explicit "other" single member)
        await notifications.SendMessageToChannelAsync(request.Dto.ChannelId, recipientUserId, messageDto);

        return new MessageResult<MessageDto>(true, Data: messageDto);
    }
}