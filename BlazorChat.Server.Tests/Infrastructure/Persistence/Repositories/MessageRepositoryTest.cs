using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Tests.Server.Infrastructure.Persistence.Repositories;

[TestFixture]
public class MessageRepositoryTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private MessageRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);

        _context.Database.EnsureCreated();

        _sut = new MessageRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Test]
    public async Task GetUnreadStatusesForUserAsync_ShouldAccuratelyEvaluateUnreadBooleans_BasedOnMessageIds()
    {
        var user = User.Create("Alice", "alice@chat.com", "pass", (u, p) => "hash");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var server = ChatServer.CreateWithDefaults("Dev Server", user.Id);
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        var channel1 = server.Channels.First();

        var channel2 = Channel.CreateServerChannel("lounge", server.Id, categoryId: null);
        _context.Channels.Add(channel2);
        await _context.SaveChangesAsync();

        var msg1 = Message.Create("Hello", channel1.Id, user.Id);
        _context.Messages.Add(msg1);
        await _context.SaveChangesAsync();

        var readState = UserChannelState.Create(user.Id, channel1.Id, msg1.Id);
        _context.UserChannelStates.Add(readState);

        var msg2 = Message.Create("Unread text here", channel2.Id, user.Id);
        _context.Messages.Add(msg2);
        await _context.SaveChangesAsync();

        var result = await _sut.GetUnreadStatusesForUserAsync(user.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2), "Should return unread tracking states for both existing channels on the server.");
            
            var status1 = result.First(x => x.ChannelId == channel1.Id);
            Assert.That(status1.HasUnreadMessages, Is.False, "Channel 1 should be marked clean because LatestMessageId == LastReadId.");

            var status2 = result.First(x => x.ChannelId == channel2.Id);
            Assert.That(status2.HasUnreadMessages, Is.True, "Channel 2 should be marked unread because LatestMessageId > LastReadId (0).");
        });
    }

    [Test]
    public async Task GetDmRecipientIdAsync_ShouldReturnTheOtherUserInsideTheDirectMessageChannel()
    {
        var sender = User.Create("Sender", "s@chat.com", "pass", (u, p) => "hash");
        var recipient = User.Create("Recipient", "r@chat.com", "pass", (u, p) => "hash");
        _context.Users.AddRange(sender, recipient);
        await _context.SaveChangesAsync();

        var dmChannel = Channel.CreateDirectMessage(sender, recipient);
        _context.Channels.Add(dmChannel);
        await _context.SaveChangesAsync();

        var resultId = await _sut.GetDmRecipientIdAsync(dmChannel.Id, sender.Id, CancellationToken.None);

        Assert.That(resultId, Is.EqualTo(recipient.Id), "Should accurately look up the DM member list and isolate the other person's ID.");
    }

    [Test]
    public async Task GetPagedMessagesAsync_WithTimeCursor_ShouldCorrectlyFetchHistoricalWindows()
    {
        var author = User.Create("Bob", "bob@chat.com", "pass", (u, p) => "hash");
        _context.Users.Add(author);
        await _context.SaveChangesAsync();

        var server = ChatServer.CreateWithDefaults("History Workspace", author.Id);
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        var channel = server.Channels.First();

        var baseTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        
        var oldMessage = Message.Create("Oldest message", channel.Id, author.Id);
        typeof(Message).GetProperty(nameof(Message.CreatedAt))!.SetValue(oldMessage, baseTime.AddMinutes(-10));
        
        var cursorTargetMessage = Message.Create("Cursor anchor point", channel.Id, author.Id);
        typeof(Message).GetProperty(nameof(Message.CreatedAt))!.SetValue(cursorTargetMessage, baseTime);

        var freshMessage = Message.Create("Newest text stream", channel.Id, author.Id);
        typeof(Message).GetProperty(nameof(Message.CreatedAt))!.SetValue(freshMessage, baseTime.AddMinutes(10));

        _context.Messages.AddRange(oldMessage, cursorTargetMessage, freshMessage);
        await _context.SaveChangesAsync();

        var results = await _sut.GetPagedMessagesAsync(
            channel.Id, 
            beforeTimestamp: baseTime, 
            exclusiveMessageId: cursorTargetMessage.Id, 
            count: 5, 
            CancellationToken.None
        );

        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Count.EqualTo(1), "Should look backwards, ignoring the cursor itself and any newer text updates.");
            Assert.That(results.First().Id, Is.EqualTo(oldMessage.Id), "Should accurately locate and return our historical records.");
        });
    }

    [Test]
    public async Task AddUserChannelStateAsync_ShouldEnforceRelationalIntegrity_AndSaveToDatabase()
    {
        var user = User.Create("Alice", "alice@chat.com", "pass", (u, p) => "hash");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var server = ChatServer.CreateWithDefaults("Test Server", user.Id);
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        var validChannelId = server.Channels.First().Id;

        var newState = UserChannelState.Create(user.Id, validChannelId, lastMessageId: 1);

        await _sut.AddUserChannelStateAsync(newState, CancellationToken.None);
        await _sut.SaveChangesAsync(CancellationToken.None);

        var verifiedState = await _context.UserChannelStates
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.ChannelId == validChannelId);
    
        Assert.Multiple(() =>
        {
            Assert.That(verifiedState, Is.Not.Null, "The read progress entry should be saved successfully.");
            Assert.That(verifiedState!.LastReadMessageId, Is.EqualTo(1));
        });
    }
}