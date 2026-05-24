using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Friends.Services;

[TestFixture]
public class FriendshipApiServiceTest
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;
    private FriendshipApiService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _sut = new FriendshipApiService(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    [Test]
    public async Task GetFriendsSummaryAsync_ReturnsFriends()
    {
        // Arrange
        var expected = new List<SidebarFriendSummaryDto> { new(1, "TestFriend", null, UserStatus.Online, 1, false) };
        _mockHttp.ExpectGet("api/friendships/sidebar-summary", expected);

        // Act
        var result = await _sut.GetFriendsSummaryAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Username, Is.EqualTo("TestFriend"));
    }

    [Test]
    public async Task GetPendingRequestsAsync_ReturnsRequests()
    {
        // Arrange
        var expected = new List<PendingFriendshipDto> { new() { RequesterId = 1, RequesterUsername = "Test" } };
        _mockHttp.ExpectGet("api/friendships/pending", expected);

        // Act
        var result = await _sut.GetPendingRequestsAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task RespondToRequestAsync_ReturnsTrue_OnSuccess()
    {
        // Arrange
        _mockHttp.ExpectPatch("api/friendships/1");

        // Act
        var result = await _sut.RespondToRequestAsync(1, true, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
    }
}
