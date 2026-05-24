using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Tests.Server.Domain.Entities;

[TestFixture]
public class MessageTests
{
    [Test]
    public void Create_WithValidParameters_ShouldInstantiateWithCorrectAndTrimmedValues()
    {
        var rawContent = "   Hello world! This is a chat message.   ";
        var channelId = 5;
        var authorId = 99;

        var message = Message.Create(rawContent, channelId, authorId);

        Assert.Multiple(() =>
        {
            Assert.That(message.Content, Is.EqualTo("Hello world! This is a chat message."), "Content should be properly trimmed upon creation.");
            Assert.That(message.ChannelId, Is.EqualTo(channelId));
            Assert.That(message.AuthorId, Is.EqualTo(authorId));
            Assert.That(message.Type, Is.EqualTo(MessageType.Text), "A message generated via the default factory should default to MessageType.Text.");
            Assert.That(message.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(message.UpdatedAt, Is.Null, "A newly created message should not have an UpdatedAt timestamp.");
            

            Assert.That(message.Channel, Is.Null);
            Assert.That(message.Author, Is.Null);
        });
    }

    [Test]
    public void Create_WithEmptyString_ShouldStillInstantiateWithEmptyContent()
    {
        var rawContent = "   ";
        var channelId = 1;
        var authorId = 2;
        
        var message = Message.Create(rawContent, channelId, authorId);
        
        Assert.That(message.Content, Is.Empty);
    }
}