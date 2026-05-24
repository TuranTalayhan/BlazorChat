using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BlazorChat.Client.Features.DirectMessage;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.DirectMessage;

[TestFixture]
public class DirectMessageApiServiceTest
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;
    private DirectMessageApiService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _sut = new DirectMessageApiService(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    [Test]
    public async Task GetMyDirectMessageChannelsAsync_ReturnsChannels()
    {
        // Arrange
        var expected = new List<ChannelDto> { new() { Id = 1, Name = "DM1" } };
        _mockHttp.ExpectGet("api/dms", expected);

        // Act
        var result = await _sut.GetMyDirectMessageChannelsAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("DM1"));
    }

    [Test]
    public async Task OpenDirectMessageAsync_ReturnsId()
    {
        // Arrange
        _mockHttp.ExpectPost("api/dms", 42);

        // Act
        var result = await _sut.OpenDirectMessageAsync(1, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(42));
    }
}
