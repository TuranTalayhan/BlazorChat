using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class UpdateChannelCommandHandler(AppDbContext db) : ICommandHandler<UpdateChannelCommand, ChannelResult<bool>>
{
    public async ValueTask<ChannelResult<bool>> Handle(UpdateChannelCommand request, CancellationToken ct)
    {
        var channel = await db.Channels
            .FirstOrDefaultAsync(c => c.Id == request.ChannelId, ct);

        if (channel == null)
            return new ChannelResult<bool>(false, Error: ChannelError.NotFound);

        if (channel.Type == ChannelType.DirectMessage)
            return new ChannelResult<bool>(false, Error: ChannelError.BadRequest, ErrorMessage: "Cannot update a Direct Message.");

        var hasPermission = await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == channel.ServerId 
                            && sm.UserId == request.CurrentUserId 
                            && sm.Role != ServerRole.Member, ct);

        if (!hasPermission)
            return new ChannelResult<bool>(false, Error: ChannelError.Forbidden);

        if (!string.IsNullOrWhiteSpace(request.Dto.Name))
            channel.Name = request.Dto.Name.Trim().ToLower();

        if (request.Dto.CategoryId.HasValue)
            channel.CategoryId = request.Dto.CategoryId.Value;

        if (request.Dto.SortOrder.HasValue)
            channel.SortOrder = request.Dto.SortOrder.Value;

        // 4. Save
        await db.SaveChangesAsync(ct);
        return new ChannelResult<bool>(true, Data: true);
    }
}