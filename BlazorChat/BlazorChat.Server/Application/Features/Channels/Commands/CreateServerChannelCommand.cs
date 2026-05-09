using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Commands;

public record CreateServerChannelCommand(
    int CurrentUserId, 
    int ServerId, 
    string Name, 
    string? CategoryName,
    int? CategoryId
) : ICommand<ChannelResult<ChannelDto>>;