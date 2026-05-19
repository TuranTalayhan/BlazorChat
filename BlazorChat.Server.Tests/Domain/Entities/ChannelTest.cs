using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Tests.Server.Domain.Entities;

[TestFixture]
public class ChannelTests
{
    [Test]
    public void CreateDirectMessage_ShouldInitializeAsDmWithMembersAndNoName()
    {
        var user1 = new User { Id = 1, Username = "Alice" };
        var user2 = new User { Id = 2, Username = "Bob" };

        var channel = Channel.CreateDirectMessage(user1, user2);

        Assert.Multiple(() =>
        {
            Assert.That(channel.Type, Is.EqualTo(ChannelType.DirectMessage));
            Assert.That(channel.Members, Has.Count.EqualTo(2));
            Assert.That(channel.Members, Contains.Item(user1));
            Assert.That(channel.Members, Contains.Item(user2));
            Assert.That(channel.Name, Is.Empty.Or.Null);
            Assert.That(channel.ServerId, Is.Null);
            Assert.That(channel.CategoryId, Is.Null);
        });
    }

    [Test]
    public void CreateServerChannel_ShouldInitializeAsServerTypeAndNormalizeName()
    {
        var inputName = "  AnNoUnCeMeNtS  ";
        var serverId = 10;
        int? categoryId = 5;

        var channel = Channel.CreateServerChannel(inputName, serverId, categoryId);

        Assert.Multiple(() =>
        {
            Assert.That(channel.Type, Is.EqualTo(ChannelType.Server));
            Assert.That(channel.Name, Is.EqualTo("announcements"), "Server channels must always be lowercased and trimmed.");
            Assert.That(channel.ServerId, Is.EqualTo(serverId));
            Assert.That(channel.CategoryId, Is.EqualTo(categoryId));
            Assert.That(channel.Members, Is.Empty);
        });
    }

    [Test]
    public void UpdateSettings_WithValidProperties_ShouldApplyAllChangesAndNormalizeName()
    {
        var channel = Channel.CreateServerChannel("general", 1, null);

        channel.UpdateSettings("  LOUNGE-chat  ", 99, 4);

        Assert.Multiple(() =>
        {
            Assert.That(channel.Name, Is.EqualTo("lounge-chat"), "Name update should be trimmed and lowercased.");
            Assert.That(channel.CategoryId, Is.EqualTo(99));
            Assert.That(channel.SortOrder, Is.EqualTo(4));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void UpdateSettings_WithNullOrWhitespaceName_ShouldKeepOriginalNameButStillUpdateOtherSettings(string? invalidName)
    {
        var initialName = "general";
        var channel = Channel.CreateServerChannel(initialName, 1, null);

        channel.UpdateSettings(invalidName, 12, 2);

        Assert.Multiple(() =>
        {
            Assert.That(channel.Name, Is.EqualTo(initialName), "An empty or whitespace string update should be ignored.");
            Assert.That(channel.CategoryId, Is.EqualTo(12));
            Assert.That(channel.SortOrder, Is.EqualTo(2));
        });
    }

    [Test]
    public void UpdateSettings_WithNullParametersForPositions_ShouldLeaveThemUnchanged()
    {
        var channel = Channel.CreateServerChannel("general", 1, 5);
        channel.UpdateSettings("new-name", null, null); // Sort order defaults to 0 on creation

        Assert.Multiple(() =>
        {
            Assert.That(channel.Name, Is.EqualTo("new-name"));
            Assert.That(channel.CategoryId, Is.EqualTo(5), "Passing null to newCategoryId should retain original value.");
            Assert.That(channel.SortOrder, Is.EqualTo(0), "Passing null to newSortOrder should retain original value.");
        });
    }

    [Test]
    public void MoveToCategory_ShouldExplicitlyAlterCategoryIdValue()
    {
        var channel = Channel.CreateServerChannel("general", 1, 5);

        channel.MoveToCategory(14);

        Assert.That(channel.CategoryId, Is.EqualTo(14));

        channel.MoveToCategory(null);

        Assert.That(channel.CategoryId, Is.Null);
    }
}