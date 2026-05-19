using System.Security.Claims;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Chat.ViewModels;

public class ChatViewModel(IChatApiService apiService, ChatAuthStateProvider auth, IChatHubService chatHubService)
    : IDisposable
{
    public List<MessageDto> Messages { get; set; } = [];
    public string CurrentMessage { get; set; } = "";
    public bool IsSending { get; set; }
    public int LoadedChannelId { get; private set; }
    public int CurrentUserId { get; private set; }
    
    public bool IsLoadingMore { get; private set; }
    public bool HasMoreMessages { get; private set; } = true;
    private const int ChunkSize = 50;

    public string ErrorMessage { get; private set; } = string.Empty;

    public event Action? OnChanged;

    public async Task InitializeAsync()
    {
        var state = await auth.GetAuthenticationStateAsync();
        int.TryParse(state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id);
        CurrentUserId = id;
        
        chatHubService.OnMessageReceived += HandleIncomingMessage;

        await chatHubService.ConnectAsync();
    }

    private void HandleIncomingMessage(MessageDto msg)
    {
        if (msg.ChannelId != LoadedChannelId) return;
        
        if (Messages.Any(m => m.Id == msg.Id)) return;
        
        Messages.Insert(0, msg);
        OnChanged?.Invoke();
        
        chatHubService.MarkAsReadAsync(msg.ChannelId, msg.Id);
    }

    public async Task LoadChannelAsync(int channelId)
    {
        if (channelId == 0) return;
        if (channelId == LoadedChannelId) return;

        if (LoadedChannelId > 0)
        {
            await chatHubService.LeaveChannelAsync(LoadedChannelId);
        }

        LoadedChannelId = channelId;
        Messages = [];
        HasMoreMessages = true;
        ErrorMessage = string.Empty; // Reset errors when switching rooms
        OnChanged?.Invoke();

        await chatHubService.JoinChannelAsync(channelId);

        var result = await apiService.GetMessagesAsync(channelId, ChunkSize, null);
    
        if (result.IsSuccess && result.Data != null)
        {
            if (result.Data.Count < ChunkSize)
            {
                HasMoreMessages = false;
            }
            Messages = result.Data; 
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Could not retrieve message logs.";
            HasMoreMessages = false;
        }
        
        OnChanged?.Invoke();
    }

    public async Task LoadNextChunkAsync()
    {
        if (IsLoadingMore || !HasMoreMessages || LoadedChannelId == 0) return;

        IsLoadingMore = true;
        OnChanged?.Invoke();

        var oldestMessage = Messages.LastOrDefault();
        DateTime? cursorTime = oldestMessage?.CreatedAt;
        int? cursorId = oldestMessage?.Id;

        var result = await apiService.GetMessagesAsync(LoadedChannelId, ChunkSize, cursorTime, cursorId);

        if (result.IsSuccess && result.Data != null)
        {
            if (result.Data.Count < ChunkSize)
            {
                HasMoreMessages = false; 
            }
            Messages.AddRange(result.Data);
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Could not load older history chunks.";
            HasMoreMessages = false;
        }

        IsLoadingMore = false;
        OnChanged?.Invoke();
    }

    public async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage) || IsSending || LoadedChannelId == 0) return;
        
        IsSending = true;
        
        var success = await apiService.SendMessageAsync(CurrentMessage, LoadedChannelId);
        if (success) CurrentMessage = "";
        
        IsSending = false;
        OnChanged?.Invoke();
    }

    public void Dispose()
    {
        chatHubService.OnMessageReceived -= HandleIncomingMessage;
    }
}