using System;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Dialogs.CreateChannel;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Dialogs.CreateChannel;

[TestFixture]
public class CreateChannelViewModelTest
{
    private IChannelsApiService _mockApiService = null!;
    private CreateChannelViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        _sut = new CreateChannelViewModel(_mockApiService);
    }

    [Test]
    public void Initialize_SetsPreSelectedCategory()
    {
        _sut.Initialize(10);
        Assert.That(_sut.HasPreSelectedCategory, Is.True);
    }

    [Test]
    public async Task SubmitAsync_SetsErrorMessage_OnApiFailure()
    {
        _sut.ChannelName = "New Channel";
        _mockApiService.CreateChannelAsync(1, Arg.Any<CreateServerChannelDto>())
            .Returns((ChannelDto?)null);

        var result = await _sut.SubmitAsync(1);

        Assert.That(result, Is.Null);
        Assert.That(_sut.ErrorMessage, Is.EqualTo("Failed to create channel. Please try again."));
    }

    [Test]
    public async Task SubmitAsync_ReturnsData_OnSuccess()
    {
        _sut.ChannelName = "New Channel";
        var expected = new ChannelDto { Id = 1, Name = "new channel" };
        _mockApiService.CreateChannelAsync(1, Arg.Any<CreateServerChannelDto>())
            .Returns(expected);

        var result = await _sut.SubmitAsync(1);

        Assert.That(result, Is.EqualTo(expected));
        Assert.That(_sut.ErrorMessage, Is.Null);
    }
}
