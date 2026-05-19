using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class GetOrCreateDmHandler(IChannelRepository channelRepository, IUserContext userContext) 
    : ICommandHandler<GetOrCreateDmCommand, ChannelResult<int>>
{
    public async ValueTask<ChannelResult<int>> Handle(GetOrCreateDmCommand request, CancellationToken ct)
    {
        var currentUserId = userContext.UserId;

        if (currentUserId == request.FriendId)
        {
            return new ChannelResult<int>(false, Error: ChannelError.BadRequest, ErrorMessage: "Cannot DM yourself.");
        }

        var existingId = await channelRepository.GetDirectMessageIdByMembersAsync(currentUserId, request.FriendId, ct);
        if (existingId.HasValue)
        {
            return new ChannelResult<int>(true, Data: existingId.Value);
        }

        var (currentUser, friendUser) = await channelRepository.GetDmUsersAsync(currentUserId, request.FriendId, ct);
        
        if (currentUser == null || friendUser == null)
        {
            return new ChannelResult<int>(false, Error: ChannelError.NotFound, ErrorMessage: "User or friend profile could not be resolved.");
        }

        var channel = Channel.CreateDirectMessage(currentUser, friendUser);

        await channelRepository.AddAsync(channel, ct);
        await channelRepository.SaveChangesAsync(ct);

        return new ChannelResult<int>(true, Data: channel.Id);
    }
}