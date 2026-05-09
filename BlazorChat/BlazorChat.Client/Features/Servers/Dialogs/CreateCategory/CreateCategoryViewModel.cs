using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Dialogs.CreateCategory;

public class CreateCategoryViewModel(IChannelApiService apiService)
{
    public string CategoryName { get; set; } = string.Empty;
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(CategoryName);

    public event Action? StateChanged;

    public async Task<CategoryDto?> SubmitAsync(int serverId)
    {
        if (!IsValid) return null;

        IsLoading = true;
        ErrorMessage = null;
        StateChanged?.Invoke();

        var dto = new CreateCategoryDto { Name = CategoryName.Trim() };
        
        var result = await apiService.CreateCategoryAsync(serverId, dto);

        if (result == null)
        {
            ErrorMessage = "Failed to create category. It may already exist.";
        }

        IsLoading = false;
        StateChanged?.Invoke();

        return result;
    }
}