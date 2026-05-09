using BlazorChat.Server.Application.Features.Channels.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class GetChannelByIdQueryHandler(AppDbContext db) : IQueryHandler<GetChannelByIdQuery, ChannelResult<ChannelDto>>
{
    public async ValueTask<ChannelResult<ChannelDto>> Handle(GetChannelByIdQuery request, CancellationToken ct)
    {
        // 1. Fetch the channel with all necessary related data
        var channel = await db.Channels
            .AsNoTracking()
            .Include(c => c.Members)   // Needed for DM Authorization and DTO mapping
            .Include(c => c.Category)  // Needed for Server Channel DTO mapping
            .FirstOrDefaultAsync(c => c.Id == request.ChannelId, ct);

        if (channel == null)
            return new ChannelResult<ChannelDto>(false, Error: ChannelError.NotFound);

        // 2. Dual Authorization Logic
        if (channel.Type == ChannelType.Server)
        {
            // For server channels, ensure the user is a member of the server
            var isServerMember = await db.ServerMemberships
                .AnyAsync(sm => sm.ServerId == channel.ServerId && sm.UserId == request.CurrentUserId, ct);
                
            if (!isServerMember)
                return new ChannelResult<ChannelDto>(false, Error: ChannelError.Forbidden);
        }
        else if (channel.Type == ChannelType.DirectMessage)
        {
            // For DMs, ensure the user is actively in the Members list of this specific channel
            var isDmMember = channel.Members.Any(m => m.Id == request.CurrentUserId);
            
            if (!isDmMember)
                return new ChannelResult<ChannelDto>(false, Error: ChannelError.Forbidden);
        }

        // 3. Map to DTO
        var dto = new ChannelDto
        {
            Id = channel.Id,
            Name = channel.Name,
            ServerId = channel.ServerId,
            SortOrder = channel.SortOrder,
            Type = (Shared.DTO.ChannelType)channel.Type,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt,
            
            // Map Members (Useful for DMs to show the other user's name)
            Members = channel.Members.Select(m => new UserDto
            {
                Id = m.Id,
                Username = m.Username,
                AvatarUrl = ""
            }).ToList(),

            // Map Category safely if it exists
            Category = channel.Category != null ? new CategoryDto 
            { 
                Id = channel.Category.Id, 
                Name = channel.Category.Name 
            } : null
        };

        return new ChannelResult<ChannelDto>(true, Data: dto);
    }
}