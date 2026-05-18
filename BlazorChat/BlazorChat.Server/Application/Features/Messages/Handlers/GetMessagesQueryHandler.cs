using BlazorChat.Server.Application.Features.Messages.Queries;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class GetMessagesQueryHandler(AppDbContext db, IChannelAuthorizationService authService) 
    : IQueryHandler<GetMessagesQuery, MessageResult<List<MessageDto>>>
{
    public async ValueTask<MessageResult<List<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        if (!await authService.CanAccessChannelAsync(request.UserId, request.ChannelId, ct))
            return new MessageResult<List<MessageDto>>(false, Error: MessageError.Forbidden, ErrorMessage: "Access denied.");
        
        var query = db.Messages.Where(m => m.ChannelId == request.ChannelId);

        if (request.BeforeTimestamp.HasValue && request.ExclusiveMessageId.HasValue)
        {
            var cursorTime = request.BeforeTimestamp.Value;
            var cursorId = request.ExclusiveMessageId.Value;

            query = query.Where(m => m.CreatedAt < cursorTime || 
                                     (m.CreatedAt == cursorTime && m.Id < cursorId));
        }
        else if (request.BeforeTimestamp.HasValue)
        {
            query = query.Where(m => m.CreatedAt < request.BeforeTimestamp.Value);
        }
        
        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Take(request.Count)                 
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                Type = (MessageType)m.Type,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                AuthorId = m.AuthorId,
                AuthorUsername = m.Author.Username,
                AuthorAvatarUrl = m.Author.AvatarUrl,
                ChannelId = m.ChannelId
            })
            .ToListAsync(ct);
        
        return new MessageResult<List<MessageDto>>(true, Data: messages);
    }
}