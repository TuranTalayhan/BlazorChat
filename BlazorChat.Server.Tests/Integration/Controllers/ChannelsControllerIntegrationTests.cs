using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Shared.DTO;
using FluentAssertions;
using NUnit.Framework;

namespace BlazorChat.Tests.Server.Integration.Controllers;

[TestFixture]
public class ChannelsControllerIntegrationTests
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

    private async Task<ChannelDto> CreateServerAndChannelAsync()
    {

        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Channel Controller Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();


        var createChannelDto = new CreateServerChannelDto { Name = "test-channel-get", CategoryId = null };
        var response = await _client.PostAsJsonAsync($"/api/servers/{server!.Id}/channels", createChannelDto);
        return (await response.Content.ReadFromJsonAsync<ChannelDto>())!;
    }

    [Test]
    public async Task GetChannel_WhenExists_ReturnsChannel()
    {
        var channel = await CreateServerAndChannelAsync();

        var response = await _client.GetAsync($"/api/channels/{channel.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChannelDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(channel.Id);
    }

    [Test]
    public async Task UpdateChannel_WhenValid_ReturnsOk()
    {
        var channel = await CreateServerAndChannelAsync();

        var updateDto = new UpdateChannelDto { Name = "updated-channel-name" };
        var response = await _client.PatchAsJsonAsync($"/api/channels/{channel.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);


        var getResponse = await _client.GetAsync($"/api/channels/{channel.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<ChannelDto>();
        result!.Name.Should().Be("updated-channel-name");
    }

    [Test]
    public async Task DeleteChannel_WhenValid_ReturnsNoContent()
    {
        var channel = await CreateServerAndChannelAsync();

        var response = await _client.DeleteAsync($"/api/channels/{channel.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);


        var getResponse = await _client.GetAsync($"/api/channels/{channel.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
