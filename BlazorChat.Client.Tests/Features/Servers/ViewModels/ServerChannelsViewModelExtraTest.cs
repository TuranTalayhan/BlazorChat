using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Features.Servers.ViewModels;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.ViewModels;

[TestFixture]
public class ServerChannelsViewModelExtraTest : BunitTestContext
{
    private IChannelsApiService _mockApiService = null!;
    private NavigationManager _mockNav = null!;
    private AppState _appState = null!;
    private NavigationState _navState = null!;
    private ServerChannelsViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        base.Setup(); // Initialize bUnit context
        _mockApiService = Substitute.For<IChannelsApiService>();
        _mockNav = TestContext.Services.GetRequiredService<NavigationManager>();
        _appState = new AppState();
        var auth = Substitute.For<AuthenticationStateProvider>();
        _navState = new NavigationState(_mockApiService, auth);

        _sut = new ServerChannelsViewModel(_mockApiService, _mockNav, _appState, _navState);
    }

    [TearDown]
    public new async Task TearDown()
    {
        _sut?.Dispose();
        _navState?.Dispose();
        await base.TearDown();
    }

    [Test]
    public void AddCategory_UpdatesCategoryList()
    {
        var category = new CategoryDto { Id = 10, Name = "New Cat" };
        _sut.AddCategory(category);
        
        var groups = _sut.CategorizedGroups.ToList();
        Assert.That(groups.Any(g => g.Key.Id == 10), Is.True);
    }

    [Test]
    public void DeleteCategory_CallsApiAndRemovesCategoryAndNullsChannels()
    {
        var category = new CategoryDto { Id = 10, Name = "Cat to delete" };
        var channel = new ChannelDto { Id = 20, Name = "Chan inside", Category = category };
        
        _sut.AddCategory(category);
        _sut.AddChannel(channel);

        _sut.DeleteCategory(10);

        _mockApiService.Received(1).DeleteCategoryAsync(0, 10);
        
        var groups = _sut.CategorizedGroups.ToList();
        Assert.That(groups.Any(g => g.Key.Id == 10), Is.False);
        
        var chan = _sut.Channels.FirstOrDefault(c => c.Id == 20);
        Assert.That(chan!.Category, Is.Null);
    }

    [Test]
    public void SetActiveChannel_UpdatesActiveIdAndAppState()
    {
        _sut.SetActiveChannel(50);
        
        Assert.That(_appState.GetLastChannelForServer(0), Is.EqualTo(50));
        Assert.That(_sut.GetChannelClass(50), Is.EqualTo("active"));
        Assert.That(_sut.GetChannelClass(99), Is.Not.EqualTo("active"));
    }
}
