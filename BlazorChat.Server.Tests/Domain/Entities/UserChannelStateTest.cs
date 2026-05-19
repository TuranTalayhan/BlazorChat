using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Tests.Server.Domain.Entities;

[TestFixture]
public class UserChannelStateTests
{
    [Test]
    public void Create_WithValidParameters_ShouldInitializeWithCorrectValues()
    {
        var userId = 5;
        var channelId = 12;
        var lastMessageId = 100;

        var state = UserChannelState.Create(userId, channelId, lastMessageId);

        Assert.Multiple(() =>
        {
            Assert.That(state.UserId, Is.EqualTo(userId));
            Assert.That(state.ChannelId, Is.EqualTo(channelId));
            Assert.That(state.LastReadMessageId, Is.EqualTo(lastMessageId), "The tracker must initialize pointing to the requested baseline message ID.");
            
            Assert.That(state.User, Is.Null);
            Assert.That(state.Channel, Is.Null);
        });
    }

    [Test]
    public void TrackProgress_WithNewerMessageId_ShouldAdvanceLastReadMessageId()
    {
        var state = UserChannelState.Create(1, 1, 100);
        var newerMessageId = 105;

        state.TrackProgress(newerMessageId);

        Assert.That(state.LastReadMessageId, Is.EqualTo(newerMessageId), "TrackProgress should smoothly advance forward when a larger message ID is recorded.");
    }

    [Test]
    public void TrackProgress_WithOlderMessageId_ShouldIgnoreMutationAndRetainHighestId()
    {
        var initialHighestId = 100;
        var state = UserChannelState.Create(1, 1, initialHighestId);
        var olderMessageId = 95;
        
        state.TrackProgress(olderMessageId);
        
        Assert.That(state.LastReadMessageId, Is.EqualTo(initialHighestId), "The domain must guard against progress tracking markers moving backwards in time.");
    }

    [Test]
    public void TrackProgress_WithIdenticalMessageId_ShouldShortCircuitWithoutAlteringState()
    {
        var state = UserChannelState.Create(1, 1, 100);

        state.TrackProgress(100);

        Assert.That(state.LastReadMessageId, Is.EqualTo(100), "Providing an identical index value should trigger an implicit state bypass.");
    }
}