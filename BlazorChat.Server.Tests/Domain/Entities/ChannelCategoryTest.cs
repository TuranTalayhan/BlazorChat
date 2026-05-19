using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Tests.Server.Domain.Entities;

[TestFixture]
public class ChannelCategoryTests
{
    [Test]
    public void Create_WithValidParameters_ShouldInstantiateWithCorrectAndTrimmedValues()
    {
        var inputName = "   Text Channels   ";
        var serverId = 42;
        var sortOrder = 3;

        var category = ChannelCategory.Create(inputName, serverId, sortOrder);

        Assert.Multiple(() =>
        {
            Assert.That(category.Name, Is.EqualTo("Text Channels"), "Name should be properly trimmed upon creation.");
            Assert.That(category.ServerId, Is.EqualTo(serverId));
            Assert.That(category.SortOrder, Is.EqualTo(sortOrder));
            Assert.That(category.Channels, Is.Not.Null);
            Assert.That(category.Channels, Is.Empty);
        });
    }

    [Test]
    public void Rename_WithValidName_ShouldUpdateAndTrimName()
    {
        var category = ChannelCategory.Create("Old Name", 1, 0);
        var newName = "   Voice Rooms   ";

        category.Rename(newName);

        Assert.That(category.Name, Is.EqualTo("Voice Rooms"), "The rename method should trim leading and trailing spaces.");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Rename_WithNullOrWhitespaceName_ShouldIgnoreMutationAndMaintainExistingName(string? invalidName)
    {
        var initialName = "Keep Me";
        var category = ChannelCategory.Create(initialName, 1, 0);

        category.Rename(invalidName!);

        Assert.That(category.Name, Is.EqualTo(initialName), "The domain should guard against empty string modifications.");
    }

    [Test]
    public void PrepareForDeletion_WithChildChannels_ShouldOrphanAllChannelsSafely()
    {
        var category = ChannelCategory.Create("Gaming", 1, 0);
        
        var channel1 = new Channel { Id = 101, Name = "cs-go", CategoryId = category.Id };
        var channel2 = new Channel { Id = 102, Name = "dota-2", CategoryId = category.Id };
        
        category.Channels.Add(channel1);
        category.Channels.Add(channel2);

        category.PrepareForDeletion();

        Assert.Multiple(() =>
        {
            Assert.That(channel1.CategoryId, Is.Null, "Channel 1 should have its category foreign key cleared out.");
            Assert.That(channel2.CategoryId, Is.Null, "Channel 2 should have its category foreign key cleared out.");
        });
    }
}