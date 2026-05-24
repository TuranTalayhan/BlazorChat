using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Authentication;

[TestFixture]
public class ChatAuthStateProviderTest
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;
    private ChatAuthStateProvider _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _sut = new ChatAuthStateProvider(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    [Test]
    public async Task GetAuthenticationStateAsync_ReturnsAnonymous_WhenStatusFails()
    {
        // Arrange
        _mockHttp.ExpectGetError("api/auth/status", HttpStatusCode.Unauthorized);

        // Act
        var state = await _sut.GetAuthenticationStateAsync();

        // Assert
        Assert.That(state.User.Identity?.IsAuthenticated, Is.False);
    }

    [Test]
    public async Task GetAuthenticationStateAsync_ReturnsAuthenticated_WhenSuccess()
    {
        // Arrange
        _mockHttp.ExpectGet("api/auth/status", new StatusDto { IsAuthenticated = true });
        _mockHttp.ExpectGet("api/auth/me", new MeDto { Id = 1, Username = "TestUser", Email = "test@test.com", Status = UserStatus.Online });

        // Act
        var state = await _sut.GetAuthenticationStateAsync();

        // Assert
        Assert.That(state.User.Identity?.IsAuthenticated, Is.True);
        Assert.That(state.User.Identity?.Name, Is.EqualTo("TestUser"));
        Assert.That(state.User.FindFirst(ClaimTypes.Email)?.Value, Is.EqualTo("test@test.com"));
    }

    [Test]
    public void NotifyUserAuthenticated_TriggersEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.AuthenticationStateChanged += _ => eventFired = true;

        var me = new MeDto { Id = 1, Username = "NewUser", Email = "new@test.com", Status = UserStatus.Idle };

        // Act
        _sut.NotifyUserAuthenticated(me);

        // Assert
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public void NotifyUserLoggedOut_TriggersEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.AuthenticationStateChanged += _ => eventFired = true;

        // Act
        _sut.NotifyUserLoggedOut();

        // Assert
        Assert.That(eventFired, Is.True);
    }
}
