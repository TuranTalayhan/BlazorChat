using System.Text.RegularExpressions;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorChat.Client.Features.Servers.ViewModels;

public class ServerChannelsViewModel : IDisposable
{
    private readonly IChannelApiService _apiService;
    private readonly NavigationManager _nav;
    public event Action? StateChanged;
    public List<ChannelDto> Channels { get; private set; } = [];
    private List<CategoryDto> Categories { get; set; } = [];
    public bool IsLoading { get; private set; }
    private int ActiveChannelId { get; set; }
    private readonly HashSet<int> _collapsedCategories = [];
    private int _currentServerId;

    public ServerChannelsViewModel(IChannelApiService apiService, NavigationManager nav)
    {
        _apiService = apiService;
        _nav = nav;
        _nav.LocationChanged += OnLocationChanged;
    }

    public async Task InitializeAsync(int serverId)
    {
        UpdateActiveChannel();
        if (_currentServerId != serverId)
        {
            _currentServerId = serverId;
            await LoadChannels(serverId);
        }
    }

    private async Task LoadChannels(int serverId)
    {
        IsLoading = true;
        StateChanged?.Invoke();
        Channels = await _apiService.GetChannelsGetByServerAsync(serverId);
        Categories = await _apiService.GetCategoriesByServerAsync(serverId);
        IsLoading = false;
        StateChanged?.Invoke();
    }

    private void UpdateActiveChannel()
    {
        var match = Regex.Match(_nav.Uri, @"/chat/(\d+)");
        ActiveChannelId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    public void ToggleCategory(int categoryId)
    {
        if (!_collapsedCategories.Add(categoryId))
            _collapsedCategories.Remove(categoryId);
    }

    public bool IsExpanded(int categoryId) => !_collapsedCategories.Contains(categoryId);

    public string GetChannelClass(int channelId) => ActiveChannelId == channelId ? "active" : "";

    public void Navigate(int id)
    {
        if (ActiveChannelId != id)
        {
            ActiveChannelId = id;
            StateChanged?.Invoke(); 
        }

        _nav.NavigateTo($"/chat/{id}");
    }

    public IEnumerable<ChannelDto> RootChannels => 
        Channels.Where(c => c.Category == null).OrderBy(c => c.SortOrder);

    public IEnumerable<IGrouping<CategoryDto, ChannelDto>> CategorizedGroups =>
        Categories
            .OrderBy(cat => cat.SortOrder)
            .Select(cat => new Grouping<CategoryDto, ChannelDto>(
                cat, 
                Channels
                    .Where(c => c.Category != null && c.Category.Id == cat.Id)
                    .OrderBy(c => c.SortOrder)
            )); 

    public void AddChannel(ChannelDto channel)
    {
        Channels.Add(channel);
    }
    
    public void AddCategory(CategoryDto newCategory)
    {
        if (Categories.Any(c => c.Id == newCategory.Id)) return;
        Categories.Add(newCategory);
        StateChanged?.Invoke();
    }
    
    public void Dispose() => _nav.LocationChanged -= OnLocationChanged;

    private void OnLocationChanged(object? s, LocationChangedEventArgs e) => UpdateActiveChannel();
    
    public class Grouping<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
    {
        public TKey Key { get; }
    
        public Grouping(TKey key, IEnumerable<TElement> elements) : base(elements)
        {
            Key = key;
        }
    }
}