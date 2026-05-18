using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Chat.Services;

public interface IChatHubService : IAsyncDisposable
{
    event Action<MessageDto>? OnMessageReceived;
    event Action<int, int>? OnReadStateUpdated;
    
    Task ConnectAsync();
    Task JoinChannelAsync(int channelId);
    Task LeaveChannelAsync(int channelId);
    Task MarkAsReadAsync(int channelId, int lastMessageId);
}

public class ChatHubService : IChatHubService
{
    private HubConnection? _hubConnection;
    public event Action<MessageDto>? OnMessageReceived;
    public event Action<int, int>? OnReadStateUpdated;

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null) return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7138/hubs/chat", options =>
            {
                options.HttpMessageHandlerFactory = innerHandler => new CookieHandler { InnerHandler = innerHandler };
            })
            .Build();

        _hubConnection.On<MessageDto>("ReceiveMessage", msg =>
        {
            OnMessageReceived?.Invoke(msg);
        });
        
        _hubConnection.On<int, int>("ReadStateUpdated", (chId, msgId) => OnReadStateUpdated?.Invoke(chId, msgId));

        await _hubConnection.StartAsync();
    }
    
    public async Task MarkAsReadAsync(int channelId, int lastMessageId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected && channelId > 0 && lastMessageId > 0)
        {
            await _hubConnection.InvokeAsync("MarkAsRead", channelId, lastMessageId);
        }
    }
    

    public async Task JoinChannelAsync(int channelId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("JoinChannel", channelId);
        }
    }

    public async Task LeaveChannelAsync(int channelId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("LeaveChannel", channelId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}