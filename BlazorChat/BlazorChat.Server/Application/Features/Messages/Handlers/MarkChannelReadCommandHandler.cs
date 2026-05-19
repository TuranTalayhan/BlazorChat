using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using Mediator;

namespace BlazorChat.Server.Application.Features.Messages.Handlers;

public class MarkChannelReadCommandHandler(IMessageRepository messageRepository) 
    : ICommandHandler<MarkChannelReadCommand, CommandResult>
{
    public async ValueTask<CommandResult> Handle(MarkChannelReadCommand request, CancellationToken ct)
    {
        var channelExists = await messageRepository.ChannelExistsAsync(request.ChannelId, ct);
        if (!channelExists)
        {
            return new CommandResult(false, "Channel not found.");
        }

        var messageExists = await messageRepository.MessageExistsInChannelAsync(request.LastMessageId, request.ChannelId, ct);
        if (!messageExists)
        {
            return new CommandResult(false, "Invalid message ID for this channel.");
        }

        var existingState = await messageRepository.GetUserChannelStateAsync(request.UserId, request.ChannelId, ct);

        if (existingState != null)
        {
            existingState.TrackProgress(request.LastMessageId);
        }
        else
        {
            var newState = UserChannelState.Create(request.UserId, request.ChannelId, request.LastMessageId);
            await messageRepository.AddUserChannelStateAsync(newState, ct);
        }

        await messageRepository.SaveChangesAsync(ct);
        
        return new CommandResult(true);
    }
}