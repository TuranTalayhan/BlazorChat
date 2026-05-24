using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BlazorChat.Tests.Server.Integration.Controllers;

[TestFixture]
public class ServersControllerIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
    }

    [SetUp]
    public void SetUp()
    {
        // By default, TestAuthHandler sets user ID=1 and username=TestUser
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task CreateServer_WhenValid_ReturnsCreatedServer()
    {
        var createDto = new CreateServerDto { Name = "Integration Test Server" };

        var response = await _client.PostAsJsonAsync("/api/servers", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var server = await response.Content.ReadFromJsonAsync<ServerDto>();
        server.Should().NotBeNull();
        server!.Name.Should().Be("Integration Test Server");
        server.Id.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GetMyServers_ReturnsListOfServers()
    {
        var response = await _client.GetAsync("/api/servers/@me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var servers = await response.Content.ReadFromJsonAsync<List<ServerDto>>();
        servers.Should().NotBeNull();
    }

    [Test]
    public async Task CreateCategory_WhenServerExists_ReturnsCreatedCategory()
    {

        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Category Test Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();


        var createCategoryDto = new CreateCategoryDto { Name = "New Integration Category" };
        var response = await _client.PostAsJsonAsync($"/api/servers/{server!.Id}/categories", createCategoryDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        category.Should().NotBeNull();
        category!.Name.Should().Be("New Integration Category");
    }

    [Test]
    public async Task CreateChannel_WhenValid_ReturnsCreatedChannel()
    {

        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Channel Test Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();


        var createChannelDto = new CreateServerChannelDto { Name = "test-channel", CategoryId = null };
        var response = await _client.PostAsJsonAsync($"/api/servers/{server!.Id}/channels", createChannelDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var channel = await response.Content.ReadFromJsonAsync<ChannelDto>();
        channel.Should().NotBeNull();
        channel!.Name.Should().Be("test-channel");
    }

    [Test]
    public async Task GetChannels_ReturnsListOfChannels()
    {

        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "GetChannels Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();


        await _client.PostAsJsonAsync($"/api/servers/{server!.Id}/channels", new CreateServerChannelDto { Name = "chan1" });


        var response = await _client.GetAsync($"/api/servers/{server.Id}/channels");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var channels = await response.Content.ReadFromJsonAsync<List<ChannelDto>>();
        channels.Should().NotBeNull();
        channels!.Count.Should().BeGreaterThan(0);
    }
}
