using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BlazorChat.Client.Features.User.Services;
using BlazorChat.Client.Features.User.ViewModels;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.User.ViewModels;

[TestFixture]
public class UserProfileViewModelTest
{
    private AuthenticationStateProvider _mockAuth = null!;
    private IUserApiService _mockApi = null!;
    private IDialogService _mockDialog = null!;
    private NavigationState _mockNavState = null!;
    private UserProfileViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockAuth = Substitute.For<AuthenticationStateProvider>();
        _mockApi = Substitute.For<IUserApiService>();
        _mockDialog = Substitute.For<IDialogService>();
        
        var mockChannelsApi = Substitute.For<BlazorChat.Client.Features.Servers.Services.IChannelsApiService>();
        _mockNavState = new NavigationState(mockChannelsApi, _mockAuth);

        _sut = new UserProfileViewModel(_mockAuth, _mockApi, _mockDialog, _mockNavState);
    }

    [TearDown]
    public void TearDown()
    {
        _sut?.Dispose();
        _mockNavState?.Dispose();
    }

    [Test]
    public async Task InitializeAsync_SetsUsernameAndStatus()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, "123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        _mockAuth.GetAuthenticationStateAsync().Returns(Task.FromResult(authState));
        _mockApi.GetMyStatusAsync(Arg.Any<CancellationToken>()).Returns(UserStatus.DoNotDisturb);

        // Act
        await _sut.InitializeAsync();

        // Assert
        Assert.That(_sut.Username, Is.EqualTo("TestUser"));
        Assert.That(_sut.CurrentStatus, Is.EqualTo(UserStatus.DoNotDisturb));
        Assert.That(_sut.StatusLabel, Is.EqualTo("Do Not Disturb"));
        Assert.That(_sut.StatusClass, Is.EqualTo("dnd"));
    }

    [Test]
    public async Task UpdateStatusAsync_CallsApiAndNavState()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "123") };
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        _mockAuth.GetAuthenticationStateAsync().Returns(Task.FromResult(authState));
        await _sut.InitializeAsync(); // Setup _myUserId

        bool eventFired = false;
        _mockNavState.OnGlobalUserStatusChanged += _ => eventFired = true;

        // Act
        await _sut.UpdateStatusAsync(UserStatus.Idle);

        // Assert
        Assert.That(_sut.CurrentStatus, Is.EqualTo(UserStatus.Idle));
        Assert.That(_sut.IsStatusPopoverOpen, Is.False);
        await _mockApi.Received(1).UpdateStatusAsync(UserStatus.Idle);
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public void ToggleStatusPopover_TogglesState()
    {
        Assert.That(_sut.IsStatusPopoverOpen, Is.False);
        _sut.ToggleStatusPopover();
        Assert.That(_sut.IsStatusPopoverOpen, Is.True);
        _sut.ToggleStatusPopover();
        Assert.That(_sut.IsStatusPopoverOpen, Is.False);
    }

    [Test]
    public void OpenSettings_ShowsDialog()
    {
        // Act
        _sut.OpenSettings();

        // Assert
        _mockDialog.Received(1).ShowAsync<BlazorChat.Client.Features.Settings.Dialogs.SettingsModal>(
            "Settings", 
            Arg.Is<DialogOptions>(o => o.CloseOnEscapeKey == true)
        );
    }

    [Test]
    public void StatusClass_And_Label_ReturnsExpectedValues()
    {
        Assert.That(_sut.GetClassForStatus(UserStatus.Online), Is.EqualTo("online"));
        Assert.That(_sut.GetClassForStatus(UserStatus.Idle), Is.EqualTo("idle"));
        Assert.That(_sut.GetClassForStatus(UserStatus.DoNotDisturb), Is.EqualTo("dnd"));
        Assert.That(_sut.GetClassForStatus(UserStatus.Offline), Is.EqualTo("offline"));
    }
}
