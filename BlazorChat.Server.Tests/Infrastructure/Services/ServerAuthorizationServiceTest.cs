using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Services;
using NSubstitute;

namespace BlazorChat.Tests.Server.Infrastructure.Services;

[TestFixture]
public class ServerAuthorizationServiceTests
{
    private IServerRepository _mockRepository = null!;
    private ServerAuthorizationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = Substitute.For<IServerRepository>();

        _sut = new ServerAuthorizationService(_mockRepository);
    }

    [TestCase(null)]
    [TestCase(0)]
    [TestCase(-5)]
    public async Task IsAdminOrOwnerAsync_WithInvalidServerId_ShouldReturnFalseImmediately(int? invalidServerId)
    {
        var userId = 99;
        
        var result = await _sut.IsAdminOrOwnerAsync(invalidServerId, userId, CancellationToken.None);
        
        Assert.That(result, Is.False, "Should instantly block access if the server parameter context is structurally invalid.");
        
        await _mockRepository.DidNotReceive().GetUserRoleInServerAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [TestCase(ServerRole.Owner)]
    [TestCase(ServerRole.Admin)]
    public async Task IsAdminOrOwnerAsync_WhenUserIsAdminOrOwner_ShouldReturnTrue(ServerRole authorizedRole)
    {
        var serverId = 10;
        var userId = 42;

        _mockRepository.GetUserRoleInServerAsync(serverId, userId, Arg.Any<CancellationToken>())
            .Returns(authorizedRole);
        var result = await _sut.IsAdminOrOwnerAsync(serverId, userId, CancellationToken.None);
        
        Assert.That(result, Is.True, $"Access should be cleanly granted if the user holds the explicit '{authorizedRole}' clearance.");
    }

    [Test]
    public async Task IsAdminOrOwnerAsync_WhenUserIsRegularMember_ShouldReturnFalse()
    {
        var serverId = 10;
        var userId = 55;

        _mockRepository.GetUserRoleInServerAsync(serverId, userId, Arg.Any<CancellationToken>())
            .Returns(ServerRole.Member);

        var result = await _sut.IsAdminOrOwnerAsync(serverId, userId, CancellationToken.None);
        
        Assert.That(result, Is.False, "Baseline standard 'Member' role profiles must be blocked from structural modification actions.");
    }

    [Test]
    public async Task IsAdminOrOwnerAsync_WhenUserHasNoMembershipRecord_ShouldReturnFalse()
    {
        var serverId = 10;
        var userId = 101;

        _mockRepository.GetUserRoleInServerAsync(serverId, userId, Arg.Any<CancellationToken>())
            .Returns((ServerRole?)null);

        var result = await _sut.IsAdminOrOwnerAsync(serverId, userId, CancellationToken.None);
        
        Assert.That(result, Is.False, "Non-members must always be safely denied entry clearance.");
    }

    [Test]
    public async Task IsAdminOrOwnerAsync_WhenUserIsBannedOrHasUnexpectedStatus_ShouldReturnFalse()
    {
        var serverId = 10;
        var userId = 666;
        
        var unexpectedRole = (ServerRole)99; 

        _mockRepository.GetUserRoleInServerAsync(serverId, userId, Arg.Any<CancellationToken>())
            .Returns(unexpectedRole);
        
        var result = await _sut.IsAdminOrOwnerAsync(serverId, userId, CancellationToken.None);
        
        Assert.That(result, Is.False, "Security check must explicitly white-list allowed roles, blocking unexpected status values.");
    }
}