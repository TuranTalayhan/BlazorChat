using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class OpenDmCommandHandler(IChannelRepository channelRepository) 
    : ICommandHandler<OpenDmCommand, ChannelResult<int>>
{
    public async ValueTask<ChannelResult<int>> Handle(OpenDmCommand request, CancellationToken ct)
    {
        if (request.CurrentUserId == request.FriendId)
            return new ChannelResult<int>(false, Error: ChannelError.BadRequest, ErrorMessage: "Cannot DM yourself.");

        var existingDm = await channelRepository.GetDirectMessageByMembersAsync(request.CurrentUserId, request.FriendId, ct);
        if (existingDm != null)
            return new ChannelResult<int>(true, Data: existingDm.Id, IsNewChannel: false);

        var usersExist = await channelRepository.UsersExistAsync(request.CurrentUserId, request.FriendId, ct);
        if (!usersExist)
            return new ChannelResult<int>(false, Error: ChannelError.NotFound, ErrorMessage: "User not found.");

        var newDm = Channel.CreateDirectMessage(request.CurrentUserId, request.FriendId);

        await channelRepository.AddAsync(newDm, ct);
        await channelRepository.SaveChangesAsync(ct);

        return new ChannelResult<int>(true, Data: newDm.Id, IsNewChannel: true);
    }
}