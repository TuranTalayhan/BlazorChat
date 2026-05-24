using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Servers.Dialogs.Invite;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Features.Servers.ViewModels;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Dialogs.Invite;

[TestFixture]
public class InviteDialogTest : BunitTestContext
{
    private ServerChannelsViewModel _viewModel = null!;
    private IChannelsApiService _mockApiService = null!;
    private NavigationState _navState = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        var auth = Substitute.For<AuthenticationStateProvider>();
        _navState = new NavigationState(_mockApiService, auth);
        
        TestContext.Services.AddSingleton(Substitute.For<ISnackbar>());
        TestContext.Services.AddMudServices();

        // NavigationManager comes from BunitTestContext
        var navManager = TestContext.Services.GetRequiredService<NavigationManager>();

        _viewModel = new ServerChannelsViewModel(
            _mockApiService,
            navManager,
            new AppState(),
            _navState
        );
    }

    [TearDown]
    public new void TearDown()
    {
        _viewModel?.Dispose();
        _navState?.Dispose();
    }

    [Test]
    public async Task Dialog_CanGenerateInvite()
    {
        var provider = TestContext.Render<MudDialogProvider>();
        var dialogService = TestContext.Services.GetRequiredService<IDialogService>();

        _mockApiService.CreateServerInviteAsync(1, Arg.Any<CreateInviteDto>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(new InviteResponseDto { Code = "INVITE123", ExpiresAt = DateTime.UtcNow.AddDays(1) });

        var parameters = new DialogParameters 
        { 
            { "ServerId", 1 },
            { "ViewModel", _viewModel }
        };

        // We must initialize the ViewModel so _currentServerId is 1 for CreateInviteAsync
        await _viewModel.InitializeAsync(1);

        IDialogReference? dialogReference = null;
        
        await provider.InvokeAsync(async () =>
        {
            dialogReference = await dialogService.ShowAsync<InviteDialog>("Invite", parameters);
        });

        Assert.That(provider.Markup, Does.Contain("Invite friends to your server"));

        var btn = provider.FindAll("button").FirstOrDefault(b => b.InnerHtml.Contains("Generate Invite Link"));
        Assert.That(btn, Is.Not.Null);

        await provider.InvokeAsync(() => btn!.Click());

        Assert.That(provider.Markup, Does.Contain("INVITE123"));
    }
}
