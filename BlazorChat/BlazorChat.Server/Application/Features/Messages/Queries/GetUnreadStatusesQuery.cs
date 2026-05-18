using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Queries;

public record GetUnreadStatusesQuery(int UserId) : IQuery<List<ChannelUnreadStatusDto>>;