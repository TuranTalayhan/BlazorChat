using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Chat.Services;

[TestFixture]
public class ChatApiServiceTest
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;
    private ChatApiService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _sut = new ChatApiService(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    [Test]
    public async Task GetMessagesAsync_ReturnsMessages_OnSuccess()
    {
        // Arrange
        var messages = new List<MessageDto> { new() { Id = 1, Content = "Hello" } };
        _mockHttp.ExpectGet("api/messages/1?count=1", messages);

        // Act
        var result = await _sut.GetMessagesAsync(1, 1, null);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task SendMessageAsync_ReturnsTrue_OnSuccess()
    {
        // Arrange
        _mockHttp.ExpectPost("api/messages", new MessageDto { Id = 1 });

        // Act
        var result = await _sut.SendMessageAsync("Hello", 1);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetMessagesAsync_ReturnsError_OnUnauthorized()
    {
        // Arrange
        _mockHttp.ExpectGetError("api/messages/1?count=1", HttpStatusCode.Unauthorized);

        // Act
        var result = await _sut.GetMessagesAsync(1, 1, null);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("expired"));
    }

    [Test]
    public async Task GetMessagesAsync_ReturnsError_OnForbidden()
    {
        // Arrange
        _mockHttp.ExpectGetError("api/messages/1?count=1", HttpStatusCode.Forbidden);

        // Act
        var result = await _sut.GetMessagesAsync(1, 1, null);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("Access denied"));
    }

    [Test]
    public async Task GetMessagesAsync_ReturnsError_OnNotFound()
    {
        // Arrange
        _mockHttp.ExpectGetError("api/messages/1?count=1", HttpStatusCode.NotFound);

        // Act
        var result = await _sut.GetMessagesAsync(1, 1, null);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("could not be found"));
    }

    [Test]
    public async Task GetUnreadStatusesAsync_ReturnsStatuses()
    {
        // Arrange
        var expected = new List<ChannelUnreadStatusDto> { new() { ChannelId = 1, HasUnreadMessages = true } };
        _mockHttp.ExpectGet("api/messages/unread-states", expected);

        // Act
        var result = await _sut.GetUnreadStatusesAsync(System.Threading.CancellationToken.None);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].ChannelId, Is.EqualTo(1));
    }
}
