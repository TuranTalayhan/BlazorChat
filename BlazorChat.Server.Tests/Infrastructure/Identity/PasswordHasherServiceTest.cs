using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Identity;

namespace BlazorChat.Tests.Server.Infrastructure.Identity;

[TestFixture]
public class PasswordHasherServiceTests
{
    private PasswordHasherService _sut = null!;
    private User _stubUser = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new PasswordHasherService();
        
        _stubUser = new User 
        { 
            Id = 1, 
            Username = "AliceTest", 
            Email = "alice@test.com" 
        };
    }

    [Test]
    public void HashPassword_WithValidInput_ShouldReturnSecureNonEmptyString()
    {
        var clearTextPassword = "MySecurePassword123!";

        var hash = _sut.HashPassword(_stubUser, clearTextPassword);

        Assert.Multiple(() =>
        {
            Assert.That(hash, Is.Not.Null.And.Not.Empty);
            Assert.That(hash, Is.Not.EqualTo(clearTextPassword), "The password must be heavily transformed and never match the clear-text raw string.");
        });
    }

    [Test]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "CorrectPasswordToMatch!";
        var secureHashedValue = _sut.HashPassword(_stubUser, password);

        var isVerified = _sut.VerifyPassword(_stubUser, secureHashedValue, password);

        Assert.That(isVerified, Is.True, "Verification must succeed when using the exact matching provided password.");
    }

    [Test]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var realPassword = "TheRealPassword123";
        var wrongPassword = "TheWrongPassword123";
        var secureHashedValue = _sut.HashPassword(_stubUser, realPassword);

        var isVerified = _sut.VerifyPassword(_stubUser, secureHashedValue, wrongPassword);

        Assert.That(isVerified, Is.False, "Verification must fail when an incorrect password attempt is processed.");
    }

    [Test]
    public void HashPassword_WhenCalledTwiceForSamePassword_ShouldProduceDifferentHashesDueToSalting()
    {
        var password = "UniversalPassword123!";

        var firstHash = _sut.HashPassword(_stubUser, password);
        var secondHash = _sut.HashPassword(_stubUser, password);

        Assert.That(firstHash, Is.Not.EqualTo(secondHash), 
            "ASP.NET Identity's PasswordHasher uses a unique random salt value per invocation. " +
            "Two identical passwords must never produce identical hashes in the database.");
    }
}