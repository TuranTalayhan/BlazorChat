using System.Text.RegularExpressions;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorChat.Client.Features.Servers;

public class ServerChannelsViewModel : IDisposable
{
    private readonly IChannelApiService _apiService;
    private readonly NavigationManager _nav;

    public ServerChannelsViewModel(IChannelApiService apiService, NavigationManager nav)
    {
        _apiService = apiService;
        _nav = nav;
        _nav.LocationChanged += OnLocationChanged;
    }

    public List<ChannelDto> Channels { get; private set; } = new();
    public bool IsLoading { get; private set; }
    public int ActiveChannelId { get; private set; }
    private HashSet<string> _collapsedCategories = new();

    public async Task InitializeAsync(int serverId)
    {
        UpdateActiveChannel();
        await LoadChannels(serverId);
    }

    public async Task LoadChannels(int serverId)
    {
        IsLoading = true;
        Channels = await _apiService.GetByServerAsync(serverId);
        IsLoading = false;
    }

    public void UpdateActiveChannel()
    {
        var match = Regex.Match(_nav.Uri, @"/chat/(\d+)");
        ActiveChannelId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    public void ToggleCategory(string categoryName)
    {
        if (!_collapsedCategories.Add(categoryName))
            _collapsedCategories.Remove(categoryName);
    }

    public bool IsExpanded(string categoryName) => !_collapsedCategories.Contains(categoryName);

    public string GetChannelClass(int channelId) => ActiveChannelId == channelId ? "active" : "";

    public void Navigate(int id) => _nav.NavigateTo($"/chat/{id}");

    public IEnumerable<ChannelDto> RootChannels => 
        Channels.Where(c => c.Category == null).OrderBy(c => c.SortOrder);

    public IEnumerable<IGrouping<string, ChannelDto>> CategorizedGroups =>
        Channels.Where(c => c.Category != null)
                .OrderBy(c => c.Category!.SortOrder)
                .ThenBy(c => c.SortOrder)
                .GroupBy(c => c.Category!.Name);

    public void Dispose() => _nav.LocationChanged -= OnLocationChanged;

    private void OnLocationChanged(object? s, LocationChangedEventArgs e) => UpdateActiveChannel();
}