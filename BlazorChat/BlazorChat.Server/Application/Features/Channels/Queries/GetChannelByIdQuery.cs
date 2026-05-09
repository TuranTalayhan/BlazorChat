using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Queries;

public record GetChannelByIdQuery(int CurrentUserId, int ChannelId) : IQuery<ChannelResult<ChannelDto>>;