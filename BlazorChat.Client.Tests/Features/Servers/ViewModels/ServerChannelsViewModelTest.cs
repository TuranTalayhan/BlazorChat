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
public class ServerChannelsViewModelTest : BunitTestContext
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
    public async Task InitializeAsync_LoadsChannelsAndCategories()
    {
        // Arrange
        _mockApiService.GetChannelsAsync(1).Returns(new List<ChannelDto>
        {
            new() { Id = 1, Name = "general", ServerId = 1, SortOrder = 1 }
        });
        _mockApiService.GetCategoriesAsync(1).Returns(new List<CategoryDto>
        {
            new() { Id = 1, Name = "Text Channels", SortOrder = 1 }
        });

        // Act
        await _sut.InitializeAsync(1);

        // Assert
        Assert.That(_sut.Channels, Has.Count.EqualTo(1));
        Assert.That(_sut.RootChannels.Count(), Is.EqualTo(1));
        Assert.That(_sut.CategorizedGroups.Count(), Is.EqualTo(1));
        Assert.That(_sut.IsLoading, Is.False);
    }

    [Test]
    public void AddChannel_UpdatesChannelList()
    {
        // Arrange
        var channel = new ChannelDto { Id = 1, Name = "new-channel", ServerId = 1 };

        // Act
        _sut.AddChannel(channel);

        // Assert
        Assert.That(_sut.Channels, Contains.Item(channel));
    }

    [Test]
    public void ToggleCategory_TogglesExpandedState()
    {
        // Act & Assert
        Assert.That(_sut.IsExpanded(1), Is.True); // Default is true
        _sut.ToggleCategory(1);
        Assert.That(_sut.IsExpanded(1), Is.False);
        _sut.ToggleCategory(1);
        Assert.That(_sut.IsExpanded(1), Is.True);
    }

    [Test]
    public void DeleteChannel_CallsApiAndRemovesFromList()
    {
        // Arrange
        var channel = new ChannelDto { Id = 1, Name = "test", ServerId = 1 };
        _sut.AddChannel(channel);

        // Act
        _sut.DeleteChannel(1);

        // Assert
        _mockApiService.Received(1).DeleteChannelAsync(0, 1); // 0 is default server id if not initialized
        Assert.That(_sut.Channels, Does.Not.Contain(channel));
    }

    [Test]
    public void StartEditingChannel_SetsEditingIdAndName()
    {
        // Act
        _sut.StartEditingChannel(new ChannelDto { Id = 1, Name = "EditingMe" });

        // Assert
        Assert.That(_sut.EditingChannelId, Is.EqualTo(1));
        Assert.That(_sut.EditName, Is.EqualTo("EditingMe"));
    }

    [Test]
    public void StartEditingCategory_SetsEditingIdAndName()
    {
        // Act
        _sut.StartEditingCategory(new CategoryDto { Id = 2, Name = "EditingCat" });

        // Assert
        Assert.That(_sut.EditingCategoryId, Is.EqualTo(2));
        Assert.That(_sut.EditingChannelId, Is.Null);
        Assert.That(_sut.EditName, Is.EqualTo("EditingCat"));
    }

    [Test]
    public async Task SaveChannelName_UpdatesNameAndCallsApi()
    {
        // Arrange
        var channel = new ChannelDto { Id = 1, Name = "OldName" };
        _sut.StartEditingChannel(channel);
        _sut.EditName = "NewName";

        // Act
        await _sut.SaveChannelName(channel);

        // Assert
        Assert.That(channel.Name, Is.EqualTo("NewName"));
        Assert.That(_sut.EditingChannelId, Is.Null);
        await _mockApiService.Received(1).UpdateChannelAsync(1, Arg.Is<UpdateChannelDto>(d => d.Name == "NewName"));
    }

    [Test]
    public async Task SaveCategoryName_UpdatesNameAndCallsApi()
    {
        // Arrange
        var category = new CategoryDto { Id = 2, Name = "OldCat" };
        _sut.StartEditingCategory(category);
        _sut.EditName = "NewCat";

        // Act
        await _sut.SaveCategoryName(category);

        // Assert
        Assert.That(category.Name, Is.EqualTo("NewCat"));
        Assert.That(_sut.EditingCategoryId, Is.Null);
        await _mockApiService.Received(1).UpdateCategoryAsync(2, Arg.Is<UpdateCategoryDto>(d => d.Name == "NewCat"));
    }
}
