using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class UpdateChannelCommandHandler(
    IChannelRepository channelRepository, 
    IServerAuthorizationService authService) 
    : ICommandHandler<UpdateChannelCommand, ChannelResult<bool>>
{
    public async ValueTask<ChannelResult<bool>> Handle(UpdateChannelCommand request, CancellationToken ct)
    {
        var channel = await channelRepository.GetByIdAsync(request.ChannelId, ct);
        if (channel == null)
        {
            return new ChannelResult<bool>(false, Error: ChannelError.NotFound);
        }

        if (channel.Type == ChannelType.DirectMessage)
        {
            return new ChannelResult<bool>(false, Error: ChannelError.BadRequest, ErrorMessage: "Cannot update a Direct Message.");
        }

        var hasPermission = await authService.IsAdminOrOwnerAsync(channel.ServerId, request.CurrentUserId, ct);
        if (!hasPermission)
        {
            return new ChannelResult<bool>(false, Error: ChannelError.Forbidden);
        }

        channel.UpdateSettings(
            request.Dto.Name, 
            request.Dto.CategoryId, 
            request.Dto.SortOrder
        );

        await channelRepository.SaveChangesAsync(ct);

        return new ChannelResult<bool>(true, Data: true);
    }
}