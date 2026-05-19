using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;

namespace BlazorChat.Tests.Server.Domain.Entities;

[TestFixture]
public class ChatServerTests
{
    [Test]
    public void CreateWithDefaults_WithValidParameters_ShouldInstantiateServerWithCorrectAndTrimmedValues()
    {
        var inputName = "   The Dev Lounge   ";
        var ownerId = 1337;

        var server = ChatServer.CreateWithDefaults(inputName, ownerId);

        Assert.Multiple(() =>
        {
            Assert.That(server.Name, Is.EqualTo("The Dev Lounge"), "Server name must be properly trimmed upon creation.");
            Assert.That(server.OwnerId, Is.EqualTo(ownerId));
            Assert.That(server.IconUrl, Is.Null, "IconUrl should default to null when a new server is initialized.");
            Assert.That(server.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(server.Owner, Is.Null, "Navigation reference properties should remain unassigned.");
            Assert.That(server.Categories, Is.Empty, "A new server should start with zero default channel categories.");
        });
    }

    [Test]
    public void CreateWithDefaults_UponExecution_ShouldAutomaticallyGenerateDefaultGeneralChannel()
    {
        var serverName = "Gaming Community";
        var ownerId = 1;

        var server = ChatServer.CreateWithDefaults(serverName, ownerId);

        Assert.Multiple(() =>
        {
            Assert.That(server.Channels, Has.Count.EqualTo(1), "A server must automatically spin up with exactly one default text channel.");
            
            var defaultChannel = server.Channels.First();
            Assert.That(defaultChannel.Name, Is.EqualTo("general"), "The core default room name must be 'general'.");
            Assert.That(defaultChannel.Type, Is.EqualTo(ChannelType.Server), "The default channel type should be a regular server channel.");
            Assert.That(defaultChannel.CategoryId, Is.Null, "The default general channel should be sitting un-categorized at the root level.");
        });
    }

    [Test]
    public void CreateWithDefaults_UponExecution_ShouldAutomaticallyRegisterCreatorAsOwner()
    {
        var serverName = "Guild Workspace";
        var ownerId = 77;

        var server = ChatServer.CreateWithDefaults(serverName, ownerId);

        Assert.Multiple(() =>
        {
            Assert.That(server.Members, Has.Count.EqualTo(1), "A server must instantly contain exactly one member upon initialization.");

            var initialMembership = server.Members.First();
            Assert.That(initialMembership.UserId, Is.EqualTo(ownerId), "The membership profile must belong directly to the creator.");
            Assert.That(initialMembership.Role, Is.EqualTo(ServerRole.Owner), "The initial creator must be granted the structural 'Owner' role authorization.");
        });
    }
}