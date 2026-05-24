using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Shared.DTO;
using FluentAssertions;
using NUnit.Framework;

namespace BlazorChat.Tests.Server.Integration.Controllers;

[TestFixture]
public class ChannelCategoriesControllerIntegrationTests
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

    private async Task<CategoryDto> CreateServerAndCategoryAsync()
    {
        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Category Controller Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();

        var response = await _client.PostAsJsonAsync($"/api/servers/{server!.Id}/categories", new CreateCategoryDto { Name = "test-category" });
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }

    [Test]
    public async Task UpdateCategory_WhenValid_ReturnsOk()
    {
        var category = await CreateServerAndCategoryAsync();

        var updateDto = new UpdateCategoryDto { Name = "updated-category" };
        var response = await _client.PatchAsJsonAsync($"/api/categories/{category.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task DeleteCategory_WhenValid_ReturnsNoContent()
    {
        var category = await CreateServerAndCategoryAsync();

        var response = await _client.DeleteAsync($"/api/categories/{category.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
