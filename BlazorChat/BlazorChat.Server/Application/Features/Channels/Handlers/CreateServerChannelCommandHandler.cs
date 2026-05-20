using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class CreateServerChannelCommandHandler(
    IChannelRepository channelRepository,
    IServerAuthorizationService authService,
    ICategoryManager categoryManager,
    IHubContext<ServerHub, IServerHubClient> hubContext) 
    : ICommandHandler<CreateServerChannelCommand, ChannelResult<ChannelDto>>
{
    public async ValueTask<ChannelResult<ChannelDto>> Handle(CreateServerChannelCommand request, CancellationToken ct)
    {
        var isAuthorized = await authService.IsAdminOrOwnerAsync(request.ServerId, request.CurrentUserId, ct);
        if (!isAuthorized)
        {
            return new ChannelResult<ChannelDto>(false, Error: ChannelError.Forbidden);
        }

        var category = await categoryManager.ResolveCategoryAsync(
            request.ServerId, request.CategoryId, request.CategoryName, ct);

        if (request.CategoryId.HasValue && category == null)
        {
            return new ChannelResult<ChannelDto>(false, Error: ChannelError.BadRequest, ErrorMessage: "Invalid Category.");
        }

        var channel = Channel.CreateServerChannel(request.Name, request.ServerId, category?.Id);

        await channelRepository.AddAsync(channel, ct);
        await channelRepository.SaveChangesAsync(ct);

        var resultDto = channel.ToDto(category);

        try
        {
            await hubContext.Clients
                .Group($"server_{request.ServerId}")
                .ChannelCreated(request.ServerId, resultDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR Realtime channel creation push failed: {ex.Message}");
        }

        return new ChannelResult<ChannelDto>(true, Data: resultDto);
    }
}