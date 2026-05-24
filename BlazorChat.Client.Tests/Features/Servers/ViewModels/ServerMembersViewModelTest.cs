using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Features.Servers.ViewModels;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.ViewModels;

[TestFixture]
public class ServerMembersViewModelTest
{
    private IChannelsApiService _mockApiService = null!;
    private NavigationState _navState = null!;
    private ServerMembersViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        var auth = Substitute.For<AuthenticationStateProvider>();
        
        var identity = new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")], "TestAuth");
        auth.GetAuthenticationStateAsync().Returns(Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(identity))));

        _navState = new NavigationState(_mockApiService, auth);
        
        _sut = new ServerMembersViewModel(_mockApiService, _navState);
    }

    [TearDown]
    public void TearDown()
    {
        _sut?.Dispose();
        _navState?.Dispose();
    }

    [Test]
    public async Task InitializeAsync_WithSelectedServer_LoadsMembers()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test" };
        
        _mockApiService.GetUserRoleInServerAsync(1).Returns(ServerRole.Owner);
        _mockApiService.GetServerMembersAsync(1).Returns(new List<UserDto>
        {
            new() { Id = 1, Username = "Member1", AvatarUrl = null }
        });

        _navState.SelectServer(server);

        // Act
        await _sut.InitializeAsync();

        // Assert
        Assert.That(_sut.Members.Count, Is.EqualTo(1));
        Assert.That(_sut.CurrentUserRole, Is.EqualTo(ServerRole.Owner));
        Assert.That(_sut.IsCurrentUserOwner, Is.True);
    }

    [Test]
    public async Task ChangeUserRoleAsync_WhenOwner_CallsApi()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test" };
        
        _mockApiService.GetUserRoleInServerAsync(1).Returns(ServerRole.Owner);
        _navState.SelectServer(server);

        await _sut.InitializeAsync(); // Sets IsCurrentUserOwner = true

        // Act
        await _sut.ChangeUserRoleAsync(2, ServerRole.Admin);

        // Assert
        await _mockApiService.Received(1).UpdateUserRoleInServerAsync(1, 2, ServerRole.Admin);
    }

    [Test]
    public async Task HandleLiveMemberJoin_AddsNewMember()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test" };
        _mockApiService.GetServerMembersAsync(1).Returns(new List<UserDto>());
        _navState.SelectServer(server);
        await _sut.InitializeAsync();

        var newUser = new UserDto { Id = 2, Username = "Newbie", AvatarUrl = null };

        // Act
        _navState.HandleUserJoinedServer(1, newUser);

        // Assert
        Assert.That(_sut.Members.Count, Is.EqualTo(1));
        Assert.That(_sut.Members[0].Username, Is.EqualTo("Newbie"));
    }

    [Test]
    public async Task HandleLivePresenceUpdate_UpdatesMemberStatus()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test" };
        _mockApiService.GetServerMembersAsync(1).Returns(new List<UserDto>
        {
            new() { Id = 2, Username = "User", Status = UserStatus.Offline, AvatarUrl = null }
        });
        _navState.SelectServer(server);
        await _sut.InitializeAsync();

        var statusUpdate = new ReceiveUserStatusDto { Id = 2, Status = UserStatus.Online };

        // Act
        _navState.HandleUserStatusChanged(statusUpdate);

        // Assert
        Assert.That(_sut.Members[0].Status, Is.EqualTo(UserStatus.Online));
    }

    [Test]
    public async Task HandleLiveRoleUpdate_UpdatesMemberRole()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test" };
        _mockApiService.GetServerMembersAsync(1).Returns(new List<UserDto>
        {
            new() { Id = 2, Username = "User", Role = ServerRole.Member, AvatarUrl = null }
        });
        _navState.SelectServer(server);
        await _sut.InitializeAsync();

        // Act
        _navState.HandleUserRoleChanged(1, 2, ServerRole.Admin);

        // Assert
        Assert.That(_sut.Members[0].Role, Is.EqualTo(ServerRole.Admin));
    }

    [Test]
    public void CSSClasses_ReturnExpectedValues()
    {
        Assert.That(_sut.GetStatusClass(UserStatus.Online), Is.EqualTo("online"));
        Assert.That(_sut.GetStatusClass(UserStatus.Idle), Is.EqualTo("away"));
        Assert.That(_sut.GetRoleCssClass(ServerRole.Owner), Is.EqualTo("role-owner"));
        Assert.That(_sut.GetRoleCssClass(ServerRole.Admin), Is.EqualTo("role-admin"));
        Assert.That(_sut.GetRoleCssClass(ServerRole.Member), Is.EqualTo("role-member"));
    }
}
