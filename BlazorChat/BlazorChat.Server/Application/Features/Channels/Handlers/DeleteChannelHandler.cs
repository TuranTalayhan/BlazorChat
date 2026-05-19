using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class DeleteChannelCommandHandler(
    IChannelRepository channelRepository, 
    IServerAuthorizationService authService) 
    : ICommandHandler<DeleteChannelCommand, ChannelResult<bool>>
{
    public async ValueTask<ChannelResult<bool>> Handle(DeleteChannelCommand request, CancellationToken ct)
    {
        var channel = await channelRepository.GetByIdAsync(request.ChannelId, ct);
        if (channel == null)
            return new ChannelResult<bool>(false, Error: ChannelError.NotFound);

        if (channel.Type == ChannelType.DirectMessage)
            return new ChannelResult<bool>(false, Error: ChannelError.BadRequest, ErrorMessage: "Cannot delete a Direct Message this way.");

        var hasPermission = await authService.IsAdminOrOwnerAsync(channel.ServerId, request.CurrentUserId, ct);
        if (!hasPermission)
            return new ChannelResult<bool>(false, Error: ChannelError.Forbidden);
        
        channelRepository.Remove(channel);
        await channelRepository.SaveChangesAsync(ct);

        return new ChannelResult<bool>(true, Data: true);
    }
}