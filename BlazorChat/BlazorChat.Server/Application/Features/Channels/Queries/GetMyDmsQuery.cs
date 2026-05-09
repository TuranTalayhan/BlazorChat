using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Queries;

public record GetMyDmsQuery(int CurrentUserId) : IQuery<List<ChannelDto>>;