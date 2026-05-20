using System.Text.RegularExpressions;
using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorChat.Client.Features.Servers.ViewModels;

public partial class ServerChannelsViewModel : IDisposable
{
    private readonly IChannelsApiService _apiService;
    private readonly NavigationManager _nav;
    private readonly NavigationState _navState;
    private readonly AppState _appState;
    
    public event Action? StateChanged;
    
    public List<ChannelDto> Channels { get; private set; } = [];
    private List<CategoryDto> Categories { get; set; } = [];
    public bool IsLoading { get; private set; }
    private int ActiveChannelId { get; set; }
    private readonly HashSet<int> _collapsedCategories = [];
    private int _currentServerId;
    public int? EditingChannelId { get; private set; }
    public int? EditingCategoryId { get; private set; }
    public string EditName { get; set; } = string.Empty;
    public ServerRole UserRole => _navState.CurrentUserRole;    
    public bool CanManageServer => UserRole is ServerRole.Admin or ServerRole.Owner;

    public ServerChannelsViewModel(
        IChannelsApiService apiService, 
        NavigationManager nav, 
        AppState appState, NavigationState navState)
    {
        _apiService = apiService;
        _nav = nav;
        _appState = appState;
        _navState = navState;
        _nav.LocationChanged += OnLocationChanged;
        navState.OnChannelStructureChanged += HandleGlobalStructureAction;
        navState.OnGlobalUserRoleChanged += HandleLiveRoleChange;
    }

    public async Task InitializeAsync(int serverId)
    {
        if (_currentServerId != serverId)
        {
            _currentServerId = serverId;
            await LoadChannels(serverId);
        }

        UpdateActiveChannel();

        if (ActiveChannelId == 0)
        {
            ActiveChannelId = _appState.GetLastChannelForServer(serverId) ?? 0;
            StateChanged?.Invoke();
        }
    }
    
    private void HandleLiveRoleChange(int serverId, int userId, ServerRole newRole)
    {
        if (_currentServerId == serverId)
        {
            StateChanged?.Invoke();
        }
    }
    
    private void HandleGlobalStructureAction(StructureActionEvent e)
    {
        if (e is { Action: ResourceAction.Created, Channel: not null })
        {
            AddChannel(e.Channel);
        }
        else if (e is { Action: ResourceAction.Deleted, TargetId: not null })
        {
            Channels.RemoveAll(c => c.Id == e.TargetId.Value);
            StateChanged?.Invoke();
        }
    }

    private async Task LoadChannels(int serverId)
    {
        IsLoading = true;
        StateChanged?.Invoke();
        
        Channels = await _apiService.GetChannelsAsync(serverId);
        Categories = await _apiService.GetCategoriesAsync(serverId);
        
        IsLoading = false;
        StateChanged?.Invoke();
    }
    
    public void SetActiveChannel(int channelId)
    {
        if (ActiveChannelId == channelId) return;
        
        ActiveChannelId = channelId;

        _appState.SaveServerChannel(_currentServerId, channelId);

        StateChanged?.Invoke();
    }
    
    public async Task<InviteResponseDto?> CreateInviteAsync(CreateInviteDto dto)
    {
        return await _apiService.CreateServerInviteAsync(_currentServerId, dto);
    }

    private void UpdateActiveChannel()
    {
        var match = MyRegex().Match(_nav.Uri);
    
        if (match.Success)
        {
            var urlChannelId = int.Parse(match.Groups[1].Value);

            var belongsToCurrentServer = Channels.Any(c => c.Id == urlChannelId);

            if (belongsToCurrentServer)
            {
                if (ActiveChannelId == urlChannelId) return;
                ActiveChannelId = urlChannelId;
                StateChanged?.Invoke();
                return;
            }
        }
        
        var cachedChannelId = _appState.GetLastChannelForServer(_currentServerId) ?? 0;

        if (ActiveChannelId == cachedChannelId) return;
        ActiveChannelId = cachedChannelId;
        StateChanged?.Invoke();
    }

    public void ToggleCategory(int categoryId)
    {
        if (!_collapsedCategories.Add(categoryId))
            _collapsedCategories.Remove(categoryId);
        StateChanged?.Invoke();
    }

    public bool IsExpanded(int categoryId) => !_collapsedCategories.Contains(categoryId);

    public string GetChannelClass(int channelId) => ActiveChannelId == channelId ? "active" : "";

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
        if (Channels.Any(c => c.Id == channel.Id)) return;
        
        Channels.Add(channel);
        
        if (channel.Category != null && Categories.All(c => c.Id != channel.Category.Id))
        {
            Categories.Add(channel.Category);
        }
        StateChanged?.Invoke();
    }
    
    public void AddCategory(CategoryDto newCategory)
    {
        if (Categories.Any(c => c.Id == newCategory.Id)) return;
        Categories.Add(newCategory);
        StateChanged?.Invoke();
    }
    
    public void StartEditingChannel(ChannelDto ch)
    {
        EditingChannelId = ch.Id;
        if (ch.Name != null) EditName = ch.Name;
        StateChanged?.Invoke();
    }

    public void StartEditingCategory(CategoryDto category)
    {
        EditingChannelId = null;
        EditingCategoryId = category.Id;
        EditName = category.Name;
        StateChanged?.Invoke();
    }

    public async Task SaveChannelName(ChannelDto channel)
    {
        if (EditingChannelId == null) return;
    
        channel.Name = EditName; 
        EditingChannelId = null;
        StateChanged?.Invoke();
        
        await _apiService.UpdateChannelAsync(channel.Id, new UpdateChannelDto { Name = EditName });
    }

    public async Task SaveCategoryName(CategoryDto category)
    {
        if (EditingCategoryId == null) return;
        
        category.Name = EditName; 
        EditingCategoryId = null;
        StateChanged?.Invoke();
        
        await _apiService.UpdateCategoryAsync(category.Id, new UpdateCategoryDto { Name = EditName });
    }

    public void DeleteChannel(int channelId)
    {
        _apiService.DeleteChannelAsync(_currentServerId, channelId);
        Channels.RemoveAll(c => c.Id == channelId);
        StateChanged?.Invoke();
    }

    public void DeleteCategory(int categoryId)
    {
        _apiService.DeleteCategoryAsync(_currentServerId, categoryId);

        foreach (var channel in Channels.Where(c => c.Category != null && c.Category.Id == categoryId))
        {
            channel.Category = null; 
        }

        Categories.RemoveAll(c => c.Id == categoryId);

        StateChanged?.Invoke();
    }

    public void Dispose()
    {
        _nav.LocationChanged -= OnLocationChanged;  
        _navState.OnChannelStructureChanged -= HandleGlobalStructureAction;
        _navState.OnGlobalUserRoleChanged -= HandleLiveRoleChange;
    } 

    private void OnLocationChanged(object? s, LocationChangedEventArgs e) => UpdateActiveChannel();
    
    private class Grouping<TKey, TElement>(TKey key, IEnumerable<TElement> elements)
        : List<TElement>(elements), IGrouping<TKey, TElement>
    {
        public TKey Key { get; } = key;
    }

    [GeneratedRegex(@"/chat/(\d+)")]
    private static partial Regex MyRegex();
}