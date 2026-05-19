using BlazorChat.Server.Application.Features.Messages.Queries;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class GetMessagesQueryHandler(IMessageRepository messageRepository, IChannelAuthorizationService authService) 
    : IQueryHandler<GetMessagesQuery, MessageResult<List<MessageDto>>>
{
    public async ValueTask<MessageResult<List<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        if (!await authService.CanAccessChannelAsync(request.UserId, request.ChannelId, ct))
        {
            return new MessageResult<List<MessageDto>>(false, Error: MessageError.Forbidden, ErrorMessage: "Access denied.");
        }
        
        var messages = await messageRepository.GetPagedMessagesAsync(
            request.ChannelId, 
            request.BeforeTimestamp, 
            request.ExclusiveMessageId, 
            request.Count, 
            ct);
        
        return new MessageResult<List<MessageDto>>(true, Data: messages);
    }
}