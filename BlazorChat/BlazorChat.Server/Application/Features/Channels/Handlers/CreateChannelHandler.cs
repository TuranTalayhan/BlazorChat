using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Server.Infrastructure.Services;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class CreateServerChannelCommandHandler(AppDbContext db, ICategoryManager categoryManager) 
    : ICommandHandler<CreateServerChannelCommand, ChannelResult<ChannelDto>>
{
    public async ValueTask<ChannelResult<ChannelDto>> Handle(CreateServerChannelCommand request, CancellationToken ct)
    {
        var membership = await db.ServerMemberships
            .FirstOrDefaultAsync(sm => sm.ServerId == request.ServerId && sm.UserId == request.CurrentUserId, ct);

        if (membership == null || membership.Role == ServerRole.Member)
            return new ChannelResult<ChannelDto>(false, Error: ChannelError.Forbidden);

        var category = await categoryManager.ResolveCategoryAsync(
            request.ServerId, request.CategoryId, request.CategoryName, ct);

        if (request.CategoryId.HasValue && category == null)
        {
            return new ChannelResult<ChannelDto>(false, Error: ChannelError.BadRequest, ErrorMessage: "Invalid Category.");
        }

        var channel = new Channel
        {
            Name = request.Name.Trim().ToLower(),
            Type = ChannelType.Server,
            ServerId = request.ServerId,
            CategoryId = category?.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Channels.Add(channel);
        await db.SaveChangesAsync(ct);

        return new ChannelResult<ChannelDto>(true, Data: channel.ToDto(category));
    }
}