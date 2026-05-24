using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Client.ViewModels;
using BlazorChat.Shared.DTO;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.ViewModels;

[TestFixture]
public class TopBarViewModelTest
{
    private IFriendshipApiService _mockApiService = null!;
    private class FakeFriendHubService : IFriendHubService
    {
        public bool ConnectCalled { get; private set; }
        public event Action<FriendshipDto>? OnNewFriendAdded;
        public event Action<PendingFriendshipDto>? OnFriendRequestReceived;

        public Task ConnectAsync()
        {
            ConnectCalled = true;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void TriggerFriendRequest(PendingFriendshipDto dto) => OnFriendRequestReceived?.Invoke(dto);
    }

    private FakeFriendHubService _fakeHubService = null!;
    private TopBarViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IFriendshipApiService>();
        _fakeHubService = new FakeFriendHubService();
        _sut = new TopBarViewModel(_mockApiService, _fakeHubService);
    }

    [TearDown]
    public async Task TearDown()
    {
        _sut?.Dispose();
        if (_fakeHubService != null) await _fakeHubService.DisposeAsync();
    }

    [Test]
    public async Task InitializeAsync_LoadsRequests_AndConnectsHub()
    {
        // Arrange
        var expected = new List<PendingFriendshipDto> { new() { RequesterId = 1, RequesterUsername = "test" } };
        _mockApiService.GetPendingRequestsAsync().Returns(expected);

        // Act
        await _sut.InitializeAsync();

        // Assert
        Assert.That(_sut.PendingRequests, Is.EqualTo(expected));
        Assert.That(_fakeHubService.ConnectCalled, Is.True);
    }

    [Test]
    public async Task HandleNewRequest_AddsRequest_AndTriggersEvent()
    {
        // Arrange
        _mockApiService.GetPendingRequestsAsync().Returns(new List<PendingFriendshipDto>());
        await _sut.InitializeAsync(); // Sets up empty list by default
        bool eventFired = false;
        string? notificationUser = null;
        _sut.OnChanged += () => eventFired = true;
        _sut.OnFriendRequestNotification += user => notificationUser = user;

        var dto = new PendingFriendshipDto { RequesterId = 2, RequesterUsername = "newUser" };

        // Act
        _fakeHubService.TriggerFriendRequest(dto);

        // Assert
        Assert.That(_sut.PendingRequests.First().RequesterId, Is.EqualTo(2));
        Assert.That(eventFired, Is.True);
        Assert.That(notificationUser, Is.EqualTo("newUser"));
    }

    [Test]
    public async Task RespondToRequestAsync_RemovesRequest_OnSuccess()
    {
        // Arrange
        var request = new PendingFriendshipDto { RequesterId = 1, RequesterUsername = "test" };
        _mockApiService.GetPendingRequestsAsync().Returns(new List<PendingFriendshipDto> { request });
        await _sut.InitializeAsync();

        _mockApiService.RespondToRequestAsync(1, true).Returns(true);

        bool eventFired = false;
        _sut.OnChanged += () => eventFired = true;

        // Act
        await _sut.RespondToRequestAsync(request, true);

        // Assert
        Assert.That(_sut.PendingRequests, Is.Empty);
        Assert.That(eventFired, Is.True);
    }
}
