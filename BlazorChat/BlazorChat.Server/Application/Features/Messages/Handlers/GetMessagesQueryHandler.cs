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
        if (!await authService.CanAccessChannelAsync(request.CurrentUserId, request.ChannelId, ct))
            return new MessageResult<List<MessageDto>>(false, Error: MessageError.Forbidden, ErrorMessage: "Access denied.");

        var messages = await db.Messages
            .Include(m => m.Author)
            .Where(m => m.ChannelId == request.ChannelId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(request.Count)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                Type = (Shared.DTO.MessageType)m.Type,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                AuthorId = m.AuthorId,
                AuthorUsername = m.Author.Username,
                AuthorAvatarUrl = m.Author.AvatarUrl,
                ChannelId = m.ChannelId
            })
            .ToListAsync(ct);

        messages.Reverse();
        return new MessageResult<List<MessageDto>>(true, Data: messages);
    }
}