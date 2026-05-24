using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Chat.Components;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Client.Features.Chat.ViewModels;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Chat.Components;

[TestFixture]
public class ChatTest : BunitTestContext
{
    private IChatApiService _mockApiService = null!;
    private IChatHubService _mockHubService = null!;
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;

    [SetUp]
    public void ChatSetUp()
    {
        _mockApiService = Substitute.For<IChatApiService>();
        _mockHubService = Substitute.For<IChatHubService>();
        
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var authStateProvider = Substitute.For<ChatAuthStateProvider>(_httpClient);
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser")
        ], "TestAuth");
        
        authStateProvider.GetAuthenticationStateAsync().Returns(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));

        TestContext.Services.AddSingleton(_httpClient);
        TestContext.Services.AddSingleton(authStateProvider);
        TestContext.Services.AddSingleton(_mockApiService);
        TestContext.Services.AddSingleton(_mockHubService);
        TestContext.Services.AddTransient<ChatViewModel>();
        
        TestContext.Services.AddSingleton(Substitute.For<NavigationState>(Substitute.For<BlazorChat.Client.Features.Servers.Services.IChannelsApiService>(), authStateProvider));
    }

    [TearDown]
    public void ChatTearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
        (_mockHubService as IDisposable)?.Dispose();
    }

    [Test]
    public void Chat_WithNoChannel_ShowsPlaceholder()
    {
        // Act
        var cut = TestContext.Render<BlazorChat.Client.Features.Chat.Components.Chat>();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Select a channel to start chatting"));
        Assert.That(cut.Markup, Does.Contain("Select a channel to start chatting"));
    }

    [Test]
    public void Chat_WithChannel_LoadsAndDisplaysMessages()
    {
        // Arrange
        _mockApiService.GetMessagesAsync(1, Arg.Any<int>(), null)
            .Returns(new ApiResponse<List<MessageDto>>
            {
                IsSuccess = true,
                Data = [
                    new MessageDto { Id = 1, Content = "Hello World!", AuthorId = 2, AuthorUsername = "User2", ChannelId = 1 },
                    new MessageDto { Id = 2, Content = "Hi there!", AuthorId = 1, AuthorUsername = "User1", ChannelId = 1 }
                ]
            });

        var cut = TestContext.Render<BlazorChat.Client.Features.Chat.Components.Chat>(
            parameters => parameters.Add(p => p.ChannelId, 1));

        // Act
        cut.WaitForState(() => cut.Markup.Contains("Hello World!"));

        // Assert
        Assert.That(cut.Markup, Does.Contain("Hello World!"));
        Assert.That(cut.Markup, Does.Contain("Hi there!"));
        _mockHubService.Received(1).JoinChannelAsync(1);
    }
}
