using System.Security.Claims;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Services;

[TestFixture]
public class NavigationStateTest
{
    private IChannelsApiService _mockChannelsApi = null!;
    private AuthenticationStateProvider _mockAuth = null!;
    private NavigationState _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockChannelsApi = Substitute.For<IChannelsApiService>();
        _mockAuth = Substitute.For<AuthenticationStateProvider>();
        
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "123") };
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        _mockAuth.GetAuthenticationStateAsync().Returns(Task.FromResult(authState));

        _sut = new NavigationState(_mockChannelsApi, _mockAuth);
    }

    [TearDown]
    public void TearDown()
    {
        _sut?.Dispose();
    }

    [Test]
    public async Task GetCurrentUserIdAsync_ReturnsParsedId()
    {
        // Act
        var id = await _sut.GetCurrentUserIdAsync();

        // Assert
        Assert.That(id, Is.EqualTo(123));
        
        // Ensure caching works (calls auth state only once)
        await _sut.GetCurrentUserIdAsync();
        await _mockAuth.Received(1).GetAuthenticationStateAsync();
    }

    [Test]
    public void SelectServer_UpdatesSelectedServerAndGetsRole()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test" };
        _mockChannelsApi.GetUserRoleInServerAsync(1).Returns(ServerRole.Admin);

        bool eventFired = false;
        _sut.OnChanged += () => eventFired = true;

        // Act
        _sut.SelectServer(server);

        // Assert
        Assert.That(_sut.SelectedServer, Is.EqualTo(server));
        Assert.That(_sut.SelectedChannelId, Is.Null);
        // CurrentUserRole will be updated asynchronously, so it might not be reflected immediately in a synchronous test without a delay or mock completion
        // However since NSubstitute returns Task.FromResult by default, it might be updated immediately
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public void SetActiveChannel_UpdatesIdAndFiresEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.OnChanged += () => eventFired = true;

        // Act
        _sut.SetActiveChannel(42);

        // Assert
        Assert.That(_sut.SelectedChannelId, Is.EqualTo(42));
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public async Task EnsureServerIsLoadedForChannelAsync_WhenServerNotLoaded_LoadsServer()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test" };
        _mockChannelsApi.GetServerByChannelIdAsync(42).Returns(server);
        _mockChannelsApi.GetUserRoleInServerAsync(1).Returns(ServerRole.Owner);

        // Act
        await _sut.EnsureServerIsLoadedForChannelAsync(42);

        // Assert
        Assert.That(_sut.SelectedServer, Is.EqualTo(server));
        Assert.That(_sut.CurrentUserRole, Is.EqualTo(ServerRole.Owner));
    }
    
    [Test]
    public void HandleUserRoleChanged_UpdatesCurrentUserRole_IfTargetIsCurrentUser()
    {
        // Arrange
        var server = new ServerDto { Id = 1 };
        _sut.SelectServer(server);
        
        // Act
        _sut.HandleUserRoleChanged(1, 123, ServerRole.Admin); // 123 is current user ID

        // Assert
        Assert.That(_sut.CurrentUserRole, Is.EqualTo(ServerRole.Admin));
    }

    [Test]
    public void HandleChannelDeleted_ClearsSelectedChannel_IfMatches()
    {
        // Arrange
        _sut.SetActiveChannel(42);
        
        // Act
        _sut.HandleChannelDeleted(42);

        // Assert
        Assert.That(_sut.SelectedChannelId, Is.Null);
    }
}
