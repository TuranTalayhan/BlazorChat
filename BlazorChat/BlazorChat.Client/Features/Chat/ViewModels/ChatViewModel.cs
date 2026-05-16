using System.Security.Claims;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Chat.ViewModels;

public class ChatViewModel(IChatApiService apiService, ChatAuthStateProvider auth) : IAsyncDisposable
{
    private HubConnection? _hub;

    public List<MessageDto> Messages { get; set; } = [];
    public string CurrentMessage { get; set; } = "";
    public bool IsSending { get; set; }
    public int LoadedChannelId { get; private set; }
    public int CurrentUserId { get; private set; }
    public event Action? OnChanged;

    public async Task InitializeAsync()
    {
        var state = await auth.GetAuthenticationStateAsync();
        int.TryParse(state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id);
        CurrentUserId = id;

        _hub = new HubConnectionBuilder()
            .WithUrl("http://localhost:7138/hubs/chat", options =>
            {
                options.HttpMessageHandlerFactory = innerHandler => new CookieHandler { InnerHandler = innerHandler };
            })
            .Build();

        _hub.On<MessageDto>("ReceiveMessage", msg =>
        {
            if (msg.ChannelId != LoadedChannelId) return;
            Messages = Messages.Prepend(msg).ToList();
            OnChanged?.Invoke();
        });

        await _hub.StartAsync();
    }

    public async Task LoadChannelAsync(int channelId)
    {
        if (channelId == LoadedChannelId) return;

        if (_hub?.State == HubConnectionState.Connected)
        {
            if (LoadedChannelId > 0) await _hub.SendAsync("LeaveChannel", LoadedChannelId);
            if (channelId > 0) await _hub.SendAsync("JoinChannel", channelId);
        }

        LoadedChannelId = channelId;
        var fetchedMessages = await apiService.GetMessagesAsync(channelId, CancellationToken.None);
        Messages = fetchedMessages.OrderByDescending(m => m.CreatedAt).ToList();
        OnChanged?.Invoke();
    }

    public async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage) || IsSending) return;
        IsSending = true;
        
        var success = await apiService.SendMessageAsync(CurrentMessage, LoadedChannelId);
        if (success) CurrentMessage = "";
        
        IsSending = false;
        OnChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null) await _hub.DisposeAsync()!;
    }
}