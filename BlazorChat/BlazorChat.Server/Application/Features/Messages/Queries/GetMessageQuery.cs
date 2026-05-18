using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Queries;

public record GetMessagesQuery(int UserId, int ChannelId, int Count, DateTime? BeforeTimestamp, int? ExclusiveMessageId) 
    : IQuery<MessageResult<List<MessageDto>>>;