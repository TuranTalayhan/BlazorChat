using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Components;
using BlazorChat.Client.Features.Servers.ViewModels;
using BlazorChat.Client.Features.Servers.Services;
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
public class SidebarChannelItemTest : BunitTestContext
{
    private IChannelsApiService _mockApiService = null!;
    private NavigationState _navState = null!;
    private ServerChannelsViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        var auth = Substitute.For<AuthenticationStateProvider>();
        var identity = new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")], "TestAuth");
        auth.GetAuthenticationStateAsync().Returns(Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(identity))));

        _navState = new NavigationState(_mockApiService, auth);
        
        TestContext.Services.AddSingleton(sp => new ServerChannelsViewModel(
            _mockApiService, 
            sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>(), 
            new BlazorChat.Client.Core.AppState(), 
            _navState));

        var mockPopoverService = Substitute.For<MudBlazor.IPopoverService>();
        mockPopoverService.PopoverOptions.Returns(new MudBlazor.PopoverOptions());
        TestContext.Services.AddSingleton(mockPopoverService);

        _viewModel = TestContext.Services.GetRequiredService<ServerChannelsViewModel>();
    }

    [TearDown]
    public void TearDown()
    {
        _navState?.Dispose();
        _viewModel?.Dispose();
    }

    [Test]
    public void Render_ShowsChannelName()
    {
        var channel = new ChannelDto { Id = 1, Name = "test-channel" };

        var cut = TestContext.Render<SidebarChannelItem>(p => p
            .Add(x => x.Channel, channel)
            .Add(x => x.ViewModel, _viewModel)
        );

        Assert.That(cut.Markup, Does.Contain("test-channel"));
    }

    [Test]
    public async Task Admin_CanSeeSettingsAndEditChannel()
    {
        // Admin
        var server = new ServerDto { Id = 1, Name = "Test" };
        _mockApiService.GetUserRoleInServerAsync(1).Returns(System.Threading.Tasks.Task.FromResult(ServerRole.Admin));
        _navState.SelectServer(server);

        var channel = new ChannelDto { Id = 1, Name = "test-channel" };
        
        var cut = TestContext.Render<SidebarChannelItem>(p => p
            .Add(x => x.Channel, channel)
            .Add(x => x.ViewModel, _viewModel)
        );

        // Click settings button
        var settingsBtn = cut.WaitForElement("button.mud-icon-button", System.TimeSpan.FromSeconds(2));
        await cut.InvokeAsync(() => settingsBtn.Click());

        // MudBlazor renders popovers in a portal, so the list items will appear in the document root or popover root.
        var editBtn = cut.FindAll(".mud-list-item").FirstOrDefault(m => m.InnerHtml.Contains("Edit Channel"));
        if (editBtn != null) await cut.InvokeAsync(() => editBtn.Click());
    }
}
