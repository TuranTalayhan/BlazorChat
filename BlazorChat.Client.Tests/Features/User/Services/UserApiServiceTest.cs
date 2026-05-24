using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BlazorChat.Client.Features.User.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.User.Services;

[TestFixture]
public class UserApiServiceTest
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;
    private UserApiService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _sut = new UserApiService(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    [Test]
    public async Task GetMyStatusAsync_ReturnsStatus()
    {
        // Arrange
        var expected = new ReceiveUserStatusDto { Status = UserStatus.Idle };
        _mockHttp.ExpectGet("api/users/me/status", expected); // Assuming ApiRoutes.Users.GetStatus is "api/users/me/status"

        // Act
        var result = await _sut.GetMyStatusAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(UserStatus.Idle));
    }

    [Test]
    public async Task UpdateStatusAsync_CallsApi()
    {
        // Arrange
        _mockHttp.ExpectPatch("api/users/me/status"); // Assuming ApiRoutes.Users.UpdateStatus is "api/users/me/status"

        // Act & Assert (just checking no throw)
        Assert.DoesNotThrowAsync(() => _sut.UpdateStatusAsync(UserStatus.DoNotDisturb));
    }
}
