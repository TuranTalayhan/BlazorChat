using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Dialogs.CreateChannel;

public class CreateChannelViewModel(IChannelApiService apiService)
{
    public string ChannelName { get; set; } = string.Empty;
    public string? Category { get; set; }

    private int? PreSelectedCategoryId { get; set; }
    public bool HasPreSelectedCategory => PreSelectedCategoryId is > 0;
    
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(ChannelName);

    public event Action? StateChanged;
    
    public void Initialize(int? categoryId)
    {
        PreSelectedCategoryId = categoryId > 0 ? categoryId : null;
    }

    public async Task<ChannelDto?> SubmitAsync(int serverId)
    {
        IsLoading = true;
        ErrorMessage = null;
        StateChanged?.Invoke();
        
        var dto = new CreateServerChannelDto() 
        { 
            Name = ChannelName.Trim().ToLower(),
            CategoryName = HasPreSelectedCategory || string.IsNullOrWhiteSpace(Category) ? null : Category.Trim(),
            CategoryId = PreSelectedCategoryId
        };

        var newChannel = await apiService.CreateChannelAsync(serverId, dto);

        if (newChannel == null)
        {
            ErrorMessage = "Failed to create channel. Please try again.";
        }

        IsLoading = false;
        StateChanged?.Invoke();

        return newChannel;
    }
}