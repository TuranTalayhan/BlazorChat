using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Commands;

public record UpdateChannelCommand(int CurrentUserId, int ChannelId, UpdateChannelDto Dto) : ICommand<ChannelResult<bool>>;