using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Services;

[TestFixture]
public class ServerApiServiceTest
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;
    private ServerApiService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _sut = new ServerApiService(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    [Test]
    public async Task GetAllMyServersAsync_ReturnsServers()
    {
        // Arrange
        var expected = new List<ServerDto> { new() { Id = 1, Name = "My Server" } };
        _mockHttp.ExpectGet("api/servers/@me", expected);

        // Act
        var result = await _sut.GetAllMyServersAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("My Server"));
    }

    [Test]
    public async Task GetAllServersAsync_ReturnsServers()
    {
        // Arrange
        var expected = new List<ServerDto> { new() { Id = 1, Name = "Public Server" } };
        _mockHttp.ExpectGet("api/servers", expected);

        // Act
        var result = await _sut.GetAllServersAsync(1);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task CreateAsync_ReturnsServer_AndTriggersEvent()
    {
        // Arrange
        var server = new ServerDto { Id = 1, Name = "New Server" };
        _mockHttp.ExpectPost("api/servers", server);

        bool eventFired = false;
        _sut.OnChanged += () => eventFired = true;

        // Act
        var result = await _sut.CreateAsync(new CreateServerDto { Name = "New Server" });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Name, Is.EqualTo("New Server"));
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public async Task CreateAsync_ReturnsNull_OnError()
    {
        // Arrange
        _mockHttp.ExpectPostError("api/servers", HttpStatusCode.BadRequest);

        // Act
        var result = await _sut.CreateAsync(new CreateServerDto { Name = "New Server" });

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void NotifyServersChanged_TriggersEvent()
    {
        // Arrange
        bool eventFired = false;
        _sut.OnChanged += () => eventFired = true;

        // Act
        _sut.NotifyServersChanged();

        // Assert
        Assert.That(eventFired, Is.True);
    }
}
