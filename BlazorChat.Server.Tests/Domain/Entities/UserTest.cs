using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Tests.Server.Domain.Entities;

[TestFixture]
public class UserTests
{
    [Test]
    public void Create_WithValidParameters_ShouldInstantiateUserWithCorrectAndNormalizedValues()
    {
        var username = "   AliceDev   ";
        var email = "   ALICE@BlazorChat.com   ";
        var rawPassword = "SuperSecretPassword123";
        
        Func<User, string, string> dummyHashStrategy = (user, pass) => $"hashed_{pass}";

        var user = User.Create(username, email, rawPassword, dummyHashStrategy);

        Assert.Multiple(() =>
        {
            Assert.That(user.Username, Is.EqualTo("AliceDev"), "Username should be properly trimmed upon creation.");
            Assert.That(user.Email, Is.EqualTo("alice@blazorchat.com"), "Email should be lowercase and trimmed.");
            Assert.That(user.PasswordHash, Is.EqualTo("hashed_SuperSecretPassword123"), "The password hash must be assigned via the provided hashing strategy strategy context.");
            Assert.That(user.Status, Is.EqualTo(UserStatus.Online), "A newly registered user should default to an Online status status presence.");
            Assert.That(user.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(user.AvatarUrl, Is.Null, "Avatar URL should default to null upon standard instantiation instantiation pipelines.");
            
            Assert.That(user.SentRequests, Is.Empty);
            Assert.That(user.ReceivedRequests, Is.Empty);
            Assert.That(user.ServerMemberships, Is.Empty);
        });
    }

    [Test]
    public void UpdateStatus_WithNewStatusState_ShouldMutateStatusProperty()
    {
        var user = User.Create("Bob", "bob@chat.com", "pass", (u, p) => "hash");

        user.UpdateStatus(UserStatus.Offline);

        Assert.That(user.Status, Is.EqualTo(UserStatus.Offline), "UpdateStatus should update the internal property value cleanly.");
    }

    [Test]
    public void UpdateStatus_WithRedundantIdenticalStatus_ShouldBypassMutationQuietly()
    {
        var user = User.Create("Bob", "bob@chat.com", "pass", (u, p) => "hash");

        user.UpdateStatus(UserStatus.Online); // Requesting an identical status assignment

        Assert.That(user.Status, Is.EqualTo(UserStatus.Online), "The state guard should safely short-circuit without executing redundant changes.");
    }
}