using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class UpdateChannelCommandHandler(
    IChannelRepository channelRepository, 
    IServerAuthorizationService authService,
    IHubContext<ServerHub, IServerHubClient> hubContext) // ADDED: Injected SignalR Hub Context
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

        try
        {
            var channelDto = new ChannelDto
            {
                Id = channel.Id,
                Name = channel.Name,
                SortOrder = channel.SortOrder,
                Category = channel.CategoryId.HasValue ? new CategoryDto { Id = channel.CategoryId.Value } : null 
            };
            
            await hubContext.Clients
                .Group($"server_{channel.ServerId}")
                .ChannelCreated(channel.ServerId, channelDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR Live Channel Update broadcast failed gracefully: {ex.Message}");
        }

        return new ChannelResult<bool>(true, Data: true);
    }
}