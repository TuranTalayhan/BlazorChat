using System.Collections.Concurrent;
using System.Reflection;
using BlazorChat.Server.Infrastructure.Services;

namespace BlazorChat.Tests.Server.Infrastructure.Services;

[TestFixture]
public class ChatPresenceTrackerTests
{
    private ChatPresenceTracker _sut = null!;
    private ConcurrentDictionary<int, HashSet<int>> _internalState = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ChatPresenceTracker();

        var fieldInfo = typeof(ChatPresenceTracker).GetField("_activeChannelUsers", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (fieldInfo == null)
        {
            Assert.Fail("Could not locate private field '_activeChannelUsers'. Check field naming conventions.");
        }

        _internalState = (ConcurrentDictionary<int, HashSet<int>>)fieldInfo!.GetValue(_sut)!;
    }

    [Test]
    public void IsUserActiveInChannel_WhenChannelDoesNotExist_ShouldReturnFalse()
    {
        var missingChannelId = 999;
        var userId = 42;

        var isActive = _sut.IsUserActiveInChannel(missingChannelId, userId);

        Assert.That(isActive, Is.False, "Should instantly return false if the requested room index hasn't been initialized.");
    }

    [Test]
    public void IsUserActiveInChannel_WhenChannelExistsButUserIsMissing_ShouldReturnFalse()
    {
        var channelId = 10;
        var activeUsersInRoom = new HashSet<int> { 1, 2, 3 };
        _internalState.TryAdd(channelId, activeUsersInRoom); // Seed the channel room data container

        var nonExistentUserId = 99;

        var isActive = _sut.IsUserActiveInChannel(channelId, nonExistentUserId);

        Assert.That(isActive, Is.False, "Should evaluate the internal hash set container and return false if the specific user ID is missing.");
    }

    [Test]
    public void IsUserActiveInChannel_WhenChannelExistsAndUserIsPresent_ShouldReturnTrue()
    {
        var channelId = 5;
        var targetedUserId = 1337;
        var activeUsersInRoom = new HashSet<int> { 100, targetedUserId, 200 };
        _internalState.TryAdd(channelId, activeUsersInRoom);

        var isActive = _sut.IsUserActiveInChannel(channelId, targetedUserId);

        Assert.That(isActive, Is.True, "Should accurately locate and confirm when an active user matches the internal channel registration layout.");
    }
}