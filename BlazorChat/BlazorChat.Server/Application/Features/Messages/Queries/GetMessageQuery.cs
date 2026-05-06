using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Queries;

public record GetMessagesQuery(int CurrentUserId, int ChannelId, int Count) : IQuery<MessageResult<List<MessageDto>>>;