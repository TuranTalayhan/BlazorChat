using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Features.Servers;

public static class ChannelMapper
{
    public static ChannelDto ToDto(this Channel channel, ChannelCategory? category)
    {
        return new ChannelDto
        {
            Id = channel.Id,
            Name = channel.Name,
            Type = channel.Type,
            ServerId = channel.ServerId,
            SortOrder = channel.SortOrder,
            Category = category != null ? new CategoryDto 
            { 
                Id = category.Id, 
                Name = category.Name,
                SortOrder = category.SortOrder
            } : null
        };
    }
}