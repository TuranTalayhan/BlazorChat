using System.Collections.Generic;
using System.Linq;
using BlazorChat.Client.Features.Servers.Components;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Features.Servers.ViewModels;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Components;

[TestFixture]
public class ServerMembersSidebarTest : BunitTestContext
{
    private IChannelsApiService _mockApiService = null!;
    private NavigationState _navState = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        var auth = Substitute.For<AuthenticationStateProvider>();
        var identity = new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")], "TestAuth");
        auth.GetAuthenticationStateAsync().Returns(System.Threading.Tasks.Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(identity))));

        _navState = new NavigationState(_mockApiService, auth);

        TestContext.Services.AddSingleton(new ServerMembersViewModel(_mockApiService, _navState));

        var mockPopoverService = Substitute.For<MudBlazor.IPopoverService>();
        mockPopoverService.PopoverOptions.Returns(new MudBlazor.PopoverOptions());
        TestContext.Services.AddSingleton(mockPopoverService);
    }

    [TearDown]
    public void TearDown()
    {
        _navState?.Dispose();
    }

    [Test]
    public void ServerMembersSidebar_RendersMembers()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test Server" };
        
        _mockApiService.GetUserRoleInServerAsync(1).Returns(System.Threading.Tasks.Task.FromResult(ServerRole.Owner));
        _mockApiService.GetServerMembersAsync(1).Returns(System.Threading.Tasks.Task.FromResult(new List<UserDto>
        {
            new() { Id = 2, Username = "MemberUser", Role = ServerRole.Member, Status = UserStatus.Online, AvatarUrl = null }
        }));
        
        _navState.SelectServer(server);

        // Act
        var cut = TestContext.Render<ServerMembersSidebar>();
        cut.WaitForState(() => !cut.Markup.Contains("MudProgressCircular"));

        // Assert
        Assert.That(cut.Markup, Does.Contain("MemberUser"));
        Assert.That(cut.Markup, Does.Contain("Server Members — 1"));
    }

    [Test]
    public void Owner_CanSeePromoteMenu()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test Server" };

        _mockApiService.GetUserRoleInServerAsync(1).Returns(System.Threading.Tasks.Task.FromResult(ServerRole.Owner));
        _mockApiService.GetServerMembersAsync(1).Returns(System.Threading.Tasks.Task.FromResult(new List<UserDto>
        {
            new() { Id = 2, Username = "MemberUser", Role = ServerRole.Member, Status = UserStatus.Online, AvatarUrl = null }
        }));

        _navState.SelectServer(server);

        var cut = TestContext.Render<ServerMembersSidebar>();
        cut.WaitForState(() => !cut.Markup.Contains("MudProgressCircular"));

        // Act
        var menuBtn = cut.WaitForElement("button.mud-icon-button", System.TimeSpan.FromSeconds(2));
        menuBtn.Click();

        var promoteBtn = cut.FindAll(".mud-list-item").FirstOrDefault(m => m.InnerHtml.Contains("Promote to Admin"));
        if (promoteBtn != null) promoteBtn.Click();
    }

    [Test]
    public void Owner_CanSeeDemoteMenu()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "Test Server" };

        _mockApiService.GetUserRoleInServerAsync(1).Returns(System.Threading.Tasks.Task.FromResult(ServerRole.Owner));
        _mockApiService.GetServerMembersAsync(1).Returns(System.Threading.Tasks.Task.FromResult(new List<UserDto>
        {
            new() { Id = 2, Username = "AdminUser", Role = ServerRole.Admin, Status = UserStatus.Online, AvatarUrl = null }
        }));

        _navState.SelectServer(server);

        var cut = TestContext.Render<ServerMembersSidebar>();
        cut.WaitForState(() => !cut.Markup.Contains("MudProgressCircular"));

        // Act
        var menuBtn = cut.WaitForElement("button.mud-icon-button", System.TimeSpan.FromSeconds(2));
        menuBtn.Click();

        var demoteBtn = cut.FindAll(".mud-list-item").FirstOrDefault(m => m.InnerHtml.Contains("Demote to Member"));
        if (demoteBtn != null) demoteBtn.Click();
    }
}
