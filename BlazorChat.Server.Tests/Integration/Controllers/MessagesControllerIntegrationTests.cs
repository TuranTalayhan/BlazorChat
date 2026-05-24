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
public class MessagesControllerIntegrationTests
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
        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Message Controller Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();

        var response = await _client.PostAsJsonAsync($"/api/servers/{server!.Id}/channels", new CreateServerChannelDto { Name = "msg-channel" });
        return (await response.Content.ReadFromJsonAsync<ChannelDto>())!;
    }

    [Test]
    public async Task SendMessage_WhenValid_ReturnsCreatedMessage()
    {
        var channel = await CreateServerAndChannelAsync();

        var sendDto = new SendMessageDto 
        { 
            ChannelId = channel.Id, 
            Content = "Hello integration test!" 
        };
        var response = await _client.PostAsJsonAsync("/api/messages", sendDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var message = await response.Content.ReadFromJsonAsync<MessageDto>();
        message.Should().NotBeNull();
        message!.Content.Should().Be("Hello integration test!");
        message.ChannelId.Should().Be(channel.Id);
        message.Id.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GetMessages_ReturnsMessagesForChannel()
    {
        var channel = await CreateServerAndChannelAsync();

        var sendDto = new SendMessageDto { ChannelId = channel.Id, Content = "Msg 1" };
        await _client.PostAsJsonAsync("/api/messages", sendDto);

        var response = await _client.GetAsync($"/api/messages/{channel.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messages = await response.Content.ReadFromJsonAsync<List<MessageDto>>();
        messages.Should().NotBeNull();
        messages!.Count.Should().BeGreaterThan(0);
        messages[0].Content.Should().Be("Msg 1");
    }

    [Test]
    public async Task GetUnreadStatuses_ReturnsListOfStatuses()
    {
        var response = await _client.GetAsync("/api/messages/unread-states");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statuses = await response.Content.ReadFromJsonAsync<List<ChannelUnreadStatusDto>>();
        statuses.Should().NotBeNull();
    }
}
