using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Client.Features.Chat.ViewModels;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Chat.ViewModels;

[TestFixture]
public class ChatViewModelTest
{
    private IChatApiService _mockApiService = null!;
    private IChatHubService _mockHubService = null!;
    private ChatAuthStateProvider _mockAuth = null!;
    private ChatViewModel _sut = null!;
    private HttpClient _mockHttpClient = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChatApiService>();
        _mockHubService = Substitute.For<IChatHubService>();
        _mockHttpClient = new HttpClient(Substitute.For<HttpMessageHandler>()) { BaseAddress = new Uri("http://localhost") };
        _mockAuth = Substitute.For<ChatAuthStateProvider>(_mockHttpClient);

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")], "TestAuth");
        _mockAuth.GetAuthenticationStateAsync().Returns(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));

        _sut = new ChatViewModel(_mockApiService, _mockAuth, _mockHubService);
    }

    [TearDown]
    public void TearDown()
    {
        _sut?.Dispose();
        _mockHttpClient?.Dispose();
        (_mockHubService as IDisposable)?.Dispose();
    }

    [Test]
    public async Task InitializeAsync_SetsUserIdAndConnectsHub()
    {
        // Act
        await _sut.InitializeAsync();

        // Assert
        Assert.That(_sut.CurrentUserId, Is.EqualTo(1));
        await _mockHubService.Received(1).ConnectAsync();
    }

    [Test]
    public async Task LoadChannelAsync_LoadsMessagesAndJoinsChannel()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { Id = 1, Content = "Test", ChannelId = 2 }
        };
        _mockApiService.GetMessagesAsync(2, Arg.Any<int>(), null)
            .Returns(new ApiResponse<List<MessageDto>> { IsSuccess = true, Data = messages });

        // Act
        await _sut.LoadChannelAsync(2);

        // Assert
        Assert.That(_sut.LoadedChannelId, Is.EqualTo(2));
        Assert.That(_sut.Messages, Has.Count.EqualTo(1));
        await _mockHubService.Received(1).JoinChannelAsync(2);
    }

    [Test]
    public async Task SendAsync_SendsMessageViaApi()
    {
        // Arrange
        _mockApiService.GetMessagesAsync(2, Arg.Any<int>(), null)
            .Returns(new ApiResponse<List<MessageDto>> { IsSuccess = true, Data = new List<MessageDto>() });
            
        await _sut.LoadChannelAsync(2);
        _sut.CurrentMessage = "Hello World";
        _mockApiService.SendMessageAsync("Hello World", 2).Returns(true);

        // Act
        await _sut.SendAsync();

        // Assert
        await _mockApiService.Received(1).SendMessageAsync("Hello World", 2);
        Assert.That(_sut.CurrentMessage, Is.Empty);
    }

    [Test]
    public async Task LoadNextChunkAsync_LoadsMoreMessages_WhenApiSucceeds()
    {
        // Arrange
        var initialMessages = Enumerable.Range(1, 50).Select(i => new MessageDto { Id = i, Content = $"Msg{i}", ChannelId = 2 }).ToList();
        _mockApiService.GetMessagesAsync(2, Arg.Any<int>(), null)
            .Returns(new ApiResponse<List<MessageDto>> { IsSuccess = true, Data = initialMessages });
            
        await _sut.LoadChannelAsync(2); // Sets LoadedChannelId = 2, HasMoreMessages = true

        var oldMessages = new List<MessageDto>
        {
            new() { Id = 99, Content = "Msg99", ChannelId = 2 }
        };
        _mockApiService.GetMessagesAsync(2, Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<int?>())
            .Returns(new ApiResponse<List<MessageDto>> { IsSuccess = true, Data = oldMessages });

        // Act
        await _sut.LoadNextChunkAsync();

        // Assert
        Assert.That(_sut.Messages, Has.Count.EqualTo(51));
        Assert.That(_sut.HasMoreMessages, Is.False); // Because oldMessages count < 50
    }

    [Test]
    public async Task LoadNextChunkAsync_SetsErrorMessage_OnApiFailure()
    {
        // Arrange
        var initialMessages = Enumerable.Range(1, 50).Select(i => new MessageDto { Id = i, Content = $"Msg{i}", ChannelId = 2 }).ToList();
        _mockApiService.GetMessagesAsync(2, Arg.Any<int>(), null)
            .Returns(new ApiResponse<List<MessageDto>> { IsSuccess = true, Data = initialMessages });
            
        await _sut.LoadChannelAsync(2);

        _mockApiService.GetMessagesAsync(2, Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<int?>())
            .Returns(new ApiResponse<List<MessageDto>> { IsSuccess = false, ErrorMessage = "Failed chunk" });

        // Act
        await _sut.LoadNextChunkAsync();

        // Assert
        Assert.That(_sut.Messages, Has.Count.EqualTo(50));
        Assert.That(_sut.ErrorMessage, Is.EqualTo("Failed chunk"));
        Assert.That(_sut.HasMoreMessages, Is.False);
    }

    [Test]
    public async Task HandleIncomingMessage_AddsMessage_WhenChannelMatches()
    {
        // Arrange
        _mockApiService.GetMessagesAsync(2, Arg.Any<int>(), null)
            .Returns(new ApiResponse<List<MessageDto>> { IsSuccess = true, Data = new List<MessageDto>() });
            
        await _sut.LoadChannelAsync(2);
        await _sut.InitializeAsync();

        var newMsg = new MessageDto { Id = 99, Content = "New", ChannelId = 2 };

        bool eventFired = false;
        _sut.OnChanged += () => eventFired = true;

        // Act
        _mockHubService.OnMessageReceived += Raise.Event<Action<MessageDto>>(newMsg);

        // Assert
        Assert.That(_sut.Messages, Contains.Item(newMsg));
        Assert.That(eventFired, Is.True);
        await _mockHubService.Received(1).MarkAsReadAsync(2, 99);
    }
}
