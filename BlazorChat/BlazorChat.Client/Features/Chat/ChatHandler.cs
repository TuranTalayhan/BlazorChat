using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Client.Features.Chat;
public record GetMessagesQuery(int ChannelId) : IQuery<List<MessageDto>>;

public record SendMessageCommand(string Content, int ChannelId) : ICommand<bool>;

public class ChatHandlers(IChatApiService chatApi) : 
    IQueryHandler<GetMessagesQuery, List<MessageDto>>, 
    ICommandHandler<SendMessageCommand, bool>
{
    public async ValueTask<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        return await chatApi.GetMessagesAsync(request.ChannelId, ct);
    }

    public async ValueTask<bool> Handle(SendMessageCommand request, CancellationToken ct)
    {
        return await chatApi.SendMessageAsync(request.Content, request.ChannelId);
    }
}