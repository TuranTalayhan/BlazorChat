using System;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Dialogs.AddServer;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Dialogs.AddServer;

[TestFixture]
public class CreateServerViewModelTest
{
    private IServerApiService _mockApiService = null!;
    private CreateServerViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IServerApiService>();
        _sut = new CreateServerViewModel(_mockApiService);
    }

    [Test]
    public void AvatarInitial_ReturnsFirstLetter_WhenNameIsSet()
    {
        _sut.ServerName = "my server";
        Assert.That(_sut.AvatarInitial, Is.EqualTo("M"));
    }

    [Test]
    public void AvatarInitial_ReturnsDefault_WhenNameIsEmpty()
    {
        _sut.ServerName = "";
        Assert.That(_sut.AvatarInitial, Is.EqualTo("S"));
    }

    [Test]
    public void CanSubmit_IsTrue_WhenNameIsSetAndNotSubmitting()
    {
        _sut.ServerName = "New Server";
        Assert.That(_sut.CanSubmit, Is.True);
    }

    [Test]
    public async Task CreateServerAsync_ReturnsNull_WhenCannotSubmit()
    {
        _sut.ServerName = "";
        var result = await _sut.CreateServerAsync();
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateServerAsync_SetsErrorMessage_WhenApiReturnsNull()
    {
        _sut.ServerName = "New Server";
        _mockApiService.CreateAsync(Arg.Any<CreateServerDto>()).Returns((ServerDto?)null);

        var result = await _sut.CreateServerAsync();

        Assert.That(result, Is.Null);
        Assert.That(_sut.ErrorMessage, Is.EqualTo("The server could not be created. Please try again."));
    }

    [Test]
    public async Task CreateServerAsync_SetsErrorMessage_WhenApiThrows()
    {
        _sut.ServerName = "New Server";
        _mockApiService.CreateAsync(Arg.Any<CreateServerDto>()).Throws(new Exception("Network Error"));

        var result = await _sut.CreateServerAsync();

        Assert.That(result, Is.Null);
        Assert.That(_sut.ErrorMessage, Is.EqualTo("A connection error occurred: Network Error"));
    }

    [Test]
    public async Task CreateServerAsync_ReturnsServer_OnSuccess()
    {
        _sut.ServerName = "New Server";
        var expected = new ServerDto { Id = 1, Name = "New Server" };
        _mockApiService.CreateAsync(Arg.Any<CreateServerDto>()).Returns(expected);

        var result = await _sut.CreateServerAsync();

        Assert.That(result, Is.EqualTo(expected));
        Assert.That(_sut.ErrorMessage, Is.Null);
    }
}
