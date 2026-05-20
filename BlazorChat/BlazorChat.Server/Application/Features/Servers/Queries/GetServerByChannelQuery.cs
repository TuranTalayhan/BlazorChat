using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Queries;

public record GetServerByChannelQuery(int ChannelId) : IQuery<ServerDto?>;