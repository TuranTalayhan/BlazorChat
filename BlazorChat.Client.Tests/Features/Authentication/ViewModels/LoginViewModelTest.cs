using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Authentication.ViewModels;
using BlazorChat.Shared.DTO;
using NSubstitute;

namespace BlazorChat.Tests.Client.Features.Authentication.ViewModels;

[TestFixture]
public class LoginViewModelTests
{
    private IAuthApiService _mockApiService = null!;
    private ICustomStateUpdater _mockStateUpdater = null!;
    private LoginViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IAuthApiService>();
        _mockStateUpdater = Substitute.For<ICustomStateUpdater>();

        _sut = new LoginViewModel(_mockApiService, _mockStateUpdater);
    }

    [Test]
    public async Task LoginUserAsync_WithValidCredentials_ShouldNotifyStateAndRaiseSuccess()
    {
        var eventFired = false;
        _sut.OnLoginSuccess += () => eventFired = true;

        var expectedUser = new MeDto { Id = 1, Username = "Alice" };
        _mockApiService.LoginAsync(_sut.Model).Returns(expectedUser);

        await _sut.LoginUserAsync();

        Assert.Multiple(() =>
        {
            Assert.That(eventFired, Is.True);
            
            _mockStateUpdater.Received(1).NotifyUserAuthenticated(expectedUser);
        });
    }
}