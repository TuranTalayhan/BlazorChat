using System.Security.Claims;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Chat.ViewModels;

public class ChatViewModel : IDisposable
{
    private readonly IChatApiService _apiService;
    private readonly ChatAuthStateProvider _auth;
    private readonly IChatHubService _chatHubService;

    public List<MessageDto> Messages { get; set; } = [];
    public string CurrentMessage { get; set; } = "";
    public bool IsSending { get; set; }
    public int LoadedChannelId { get; private set; }
    public int CurrentUserId { get; private set; }
    
    public event Action? OnChanged;

    public ChatViewModel(IChatApiService apiService, ChatAuthStateProvider auth, IChatHubService chatHubService)
    {
        _apiService = apiService;
        _auth = auth;
        _chatHubService = chatHubService;

        _chatHubService.OnMessageReceived += HandleIncomingMessage;
    }

    public async Task InitializeAsync()
    {
        var state = await _auth.GetAuthenticationStateAsync();
        int.TryParse(state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id);
        CurrentUserId = id;

        await _chatHubService.ConnectAsync();
    }

    private void HandleIncomingMessage(MessageDto msg)
    {
        if (msg.ChannelId != LoadedChannelId) return;
        
        Messages = Messages.Prepend(msg).ToList();
        OnChanged?.Invoke();
        
        _chatHubService.MarkAsReadAsync(msg.ChannelId, msg.Id);
    }

    public async Task LoadChannelAsync(int channelId)
    {
        if (channelId == LoadedChannelId) return;

        if (LoadedChannelId > 0) await _chatHubService.LeaveChannelAsync(LoadedChannelId);

        LoadedChannelId = channelId;

        if (channelId == 0)
        {
            Messages = [];
            CurrentMessage = "";
            OnChanged?.Invoke();
            return;
        }

        await _chatHubService.JoinChannelAsync(channelId);

        var fetchedMessages = await _apiService.GetMessagesAsync(channelId, CancellationToken.None);
        Messages = fetchedMessages.OrderByDescending(m => m.CreatedAt).ToList();
        OnChanged?.Invoke();

        var latestMessage = Messages.FirstOrDefault();
        if (latestMessage != null)
        {
            await _chatHubService.MarkAsReadAsync(channelId, latestMessage.Id);
        }
    }

    public async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage) || IsSending || LoadedChannelId == 0) return;
        
        IsSending = true;
        
        var success = await _apiService.SendMessageAsync(CurrentMessage, LoadedChannelId);
        if (success) CurrentMessage = "";
        
        IsSending = false;
        OnChanged?.Invoke();
    }

    public void Dispose()
    {
        _chatHubService.OnMessageReceived -= HandleIncomingMessage;
    }
}