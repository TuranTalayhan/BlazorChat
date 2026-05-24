using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Tests.Server.Domain.Entities;

[TestFixture]
public class FriendshipTests
{
    [Test]
    public void CreatePending_ShouldInitializeWithCorrectIdsAndPendingStatus()
    {
        var requesterId = 10;
        var receiverId = 20;

        var friendship = Friendship.CreatePending(requesterId, receiverId);

        Assert.Multiple(() =>
        {
            Assert.That(friendship.RequesterId, Is.EqualTo(requesterId));
            Assert.That(friendship.ReceiverId, Is.EqualTo(receiverId));
            Assert.That(friendship.Status, Is.EqualTo(FriendshipStatus.Pending), "A newly initialized request must start as Pending.");
            Assert.That(friendship.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(friendship.Requester, Is.Null, "Navigation stubs should remain unassigned upon factory invocation.");
            Assert.That(friendship.Receiver, Is.Null, "Navigation stubs should remain unassigned upon factory invocation.");
        });
    }

    [Test]
    public void Accept_WhenStatusIsPending_ShouldTransitionToAccepted()
    {
        var friendship = Friendship.CreatePending(1, 2);

        friendship.Accept();

        Assert.That(friendship.Status, Is.EqualTo(FriendshipStatus.Accepted), "Accepting a pending request should update the status state to Accepted.");
    }

    [Test]
    public void Accept_WhenStatusIsAlreadyAccepted_ShouldIgnoreTransitionAndMaintainAcceptedStatus()
    {
        var friendship = Friendship.CreatePending(1, 2);
        friendship.Accept();
        
        friendship.Accept(); // Execute a redundant second invocation

        Assert.That(friendship.Status, Is.EqualTo(FriendshipStatus.Accepted), "The domain should guard against duplicate execution modifications.");
    }

    [Test]
    public void Accept_WhenStatusIsBlocked_ShouldIgnoreTransitionAndMaintainBlockedStatus()
    {
        var friendship = Friendship.CreatePending(1, 2);
        friendship.Status = FriendshipStatus.Blocked; // Artificially advance state machine to Blocked

        friendship.Accept();

        Assert.That(friendship.Status, Is.EqualTo(FriendshipStatus.Blocked), "A blocked relationship state boundary should never be overriden by an Accept request.");
    }
}