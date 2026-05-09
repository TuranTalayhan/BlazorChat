namespace BlazorChat.Server.Application.Features.Channels;

public record ChannelResult<T>(
    bool IsSuccess, 
    T? Data = default, 
    ChannelError Error = ChannelError.None, 
    string? ErrorMessage = null,
    bool IsNewChannel = false
);