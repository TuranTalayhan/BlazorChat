using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Client.Features.DirectMessage;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Client.Features.Friends.ViewModels;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Friends.ViewModels;

[TestFixture]
public class FriendsSidebarViewModelTest
{
    private IFriendshipApiService _mockFriendshipApi = null!;
    private IDirectMessageApiService _mockDmApi = null!;
    private IGlobalNotificationService _mockNotifications = null!;
    private AppState _appState = null!;
    private IChatHubService _mockChatHub = null!;
    private NavigationState _navState = null!;
    private FriendsSidebarViewModel _sut = null!;

    private class FakeNavigationManager : NavigationManager
    {
        public string? NavigatedTo { get; private set; }
        public FakeNavigationManager() => Initialize("http://localhost/", "http://localhost/");
        protected override void NavigateToCore(string uri, bool forceLoad) => NavigatedTo = uri;
    }

    private FakeNavigationManager _fakeNavManager = null!;

    [SetUp]
    public void SetUp()
    {
        _mockFriendshipApi = Substitute.For<IFriendshipApiService>();
        _mockDmApi = Substitute.For<IDirectMessageApiService>();
        _mockNotifications = Substitute.For<IGlobalNotificationService>();
        _fakeNavManager = new FakeNavigationManager();
        _appState = new AppState();
        _mockChatHub = Substitute.For<IChatHubService>();
        var mockChannelsApi = Substitute.For<IChannelsApiService>();
        var mockAuth = Substitute.For<AuthenticationStateProvider>();
        _navState = new NavigationState(mockChannelsApi, mockAuth);

        _sut = new FriendsSidebarViewModel(
            _mockFriendshipApi,
            _mockDmApi,
            _mockNotifications,
            _fakeNavManager,
            _appState,
            _mockChatHub,
            _navState);
    }

    [TearDown]
    public void TearDown()
    {
        _sut?.Dispose();
        _navState?.Dispose();
        (_mockChatHub as IDisposable)?.Dispose();
    }

    [Test]
    public async Task InitializeAsync_LoadsFriends_AndConnectsNotifications()
    {
        // Arrange
        _mockFriendshipApi.GetFriendsSummaryAsync().Returns(new List<SidebarFriendSummaryDto>
        {
            new SidebarFriendSummaryDto(1, "Friend1", null, UserStatus.Online, 1, false)
        });

        // Act
        await _sut.InitializeAsync();

        // Assert
        Assert.That(_sut.FilteredFriends.Count(), Is.EqualTo(1));
        await _mockNotifications.Received(1).EnsureConnectedAsync();
    }

    [Test]
    public async Task OpenChatWithFriend_NavigatesToChat_WhenChannelExists()
    {
        // Arrange
        _mockFriendshipApi.GetFriendsSummaryAsync().Returns(new List<SidebarFriendSummaryDto>
        {
            new SidebarFriendSummaryDto(1, "Friend1", null, UserStatus.Online, 10, false)
        });
        await _sut.InitializeAsync();

        // Act
        await _sut.OpenChatWithFriend(1);

        // Assert
        Assert.That(_fakeNavManager.NavigatedTo, Is.EqualTo("/chat/10"));
        Assert.That(_sut.ActiveFriendId, Is.EqualTo(1));
    }

    [Test]
    public async Task OpenChatWithFriend_CreatesChannel_WhenChannelDoesNotExist()
    {
        // Arrange
        _mockFriendshipApi.GetFriendsSummaryAsync().Returns(new List<SidebarFriendSummaryDto>
        {
            new SidebarFriendSummaryDto(1, "Friend1", null, UserStatus.Online, 0, false)
        });
        await _sut.InitializeAsync();

        _mockDmApi.OpenDirectMessageAsync(1).Returns(42);

        // Act
        await _sut.OpenChatWithFriend(1);

        // Assert
        await _mockDmApi.Received(1).OpenDirectMessageAsync(1);
        Assert.That(_fakeNavManager.NavigatedTo, Is.EqualTo("/chat/42"));
        Assert.That(_sut.ActiveFriendId, Is.EqualTo(1));
    }

    [Test]
    public async Task SetActiveFilter_UpdatesStatusFilter()
    {
        // Arrange
        _mockFriendshipApi.GetFriendsSummaryAsync().Returns(new List<SidebarFriendSummaryDto>
        {
            new SidebarFriendSummaryDto(1, "Friend1", null, UserStatus.Online, 0, false),
            new SidebarFriendSummaryDto(2, "Friend2", null, UserStatus.Offline, 0, false)
        });
        await _sut.InitializeAsync();

        // Act
        _sut.SetActiveFilter(FriendFilter.Online); // Actually it's FriendFilter.All vs Online

        // Assert
        Assert.That(_sut.FilteredFriends.Count(), Is.EqualTo(1));
        Assert.That(_sut.FilteredFriends.First().FriendId, Is.EqualTo(1));
    }

    [Test]
    public void GetStatusClass_ReturnsCorrectString()
    {
        Assert.That(FriendsSidebarViewModel.GetStatusClass(UserStatus.Online), Is.EqualTo("online"));
        Assert.That(FriendsSidebarViewModel.GetStatusClass(UserStatus.Idle), Is.EqualTo("idle"));
        Assert.That(FriendsSidebarViewModel.GetStatusClass(UserStatus.DoNotDisturb), Is.EqualTo("dnd"));
        Assert.That(FriendsSidebarViewModel.GetStatusClass(UserStatus.Offline), Is.EqualTo("offline"));
    }
}
