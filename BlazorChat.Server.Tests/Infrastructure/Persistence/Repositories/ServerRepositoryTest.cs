using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Repositories;
using BlazorChat.Shared.Enums;

namespace BlazorChat.Tests.Server.Infrastructure.Persistence.Repositories;

[TestFixture]
public class ServerRepositoryTests : SqliteTestBase<AppDbContext>
{
    private ServerRepository _sut = null!;

    [SetUp]
    public void InitSut()
    {
        _sut = new ServerRepository(Context);
    }

    [Test]
    public async Task GetUserRoleInServerAsync_WhenMembershipExists_ShouldReturnCorrectRole()
    {
        var user = User.Create("Alice", "alice@chat.com", "pass", (u, p) => "hash");
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var server = ChatServer.CreateWithDefaults("Dev Room", user.Id);
        Context.Servers.Add(server);
        await Context.SaveChangesAsync();

        var role = await _sut.GetUserRoleInServerAsync(server.Id, user.Id, CancellationToken.None);


        Assert.That(role, Is.EqualTo(ServerRole.Owner), "The creator should be found with the Owner role.");
    }

    [Test]
    public async Task GetChannelsByServerIdAsync_ShouldReturnMappedDtosOrderedBySortOrderAndCreation()
    {
        var user = User.Create("Bob", "bob@chat.com", "pass", (u, p) => "hash");
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var server = ChatServer.CreateWithDefaults("Workspace", user.Id);
        Context.Servers.Add(server);
        await Context.SaveChangesAsync();

        var category = ChannelCategory.Create("TEXT", server.Id, 0);
        Context.ChannelCategories.Add(category);
        await Context.SaveChangesAsync();

        var channelHighPriority = Channel.CreateServerChannel("announcements", server.Id, category.Id);
        channelHighPriority.SortOrder = -1; // Should come first
        
        var channelLowPriority = Channel.CreateServerChannel("random", server.Id, category.Id);
        channelLowPriority.SortOrder = 5;  // Should come last

        Context.Channels.AddRange(channelHighPriority, channelLowPriority);
        await Context.SaveChangesAsync();

        var channels = await _sut.GetChannelsByServerIdAsync(server.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(channels, Has.Count.GreaterThanOrEqualTo(2));
            var sortedList = channels.Where(c => c.Name == "announcements" || c.Name == "random").ToList();
            
            Assert.That(sortedList[0].Name, Is.EqualTo("announcements"));
            Assert.That(sortedList[0].Category, Is.Not.Null);
            Assert.That(sortedList[0].Category!.Name, Is.EqualTo("TEXT"));
            Assert.That(sortedList[1].Name, Is.EqualTo("random"));
        });
    }

    [Test]
    public async Task IsMemberAsync_ShouldReturnTrueIfUserExistsInServerAndFalseIfNot()
    {
        var user = User.Create("Charlie", "charlie@chat.com", "pass", (u, p) => "hash");
        var nonMember = User.Create("Stranger", "stranger@chat.com", "pass", (u, p) => "hash");
        Context.Users.AddRange(user, nonMember);
        await Context.SaveChangesAsync();

        var server = ChatServer.CreateWithDefaults("Guild", user.Id);
        Context.Servers.Add(server);
        await Context.SaveChangesAsync();
        
        var isMemberTrue = await _sut.IsMemberAsync(server.Id, user.Id, CancellationToken.None);
        var isMemberFalse = await _sut.IsMemberAsync(server.Id, nonMember.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(isMemberTrue, Is.True);
            Assert.That(isMemberFalse, Is.False);
        });
    }

    [Test]
    public async Task GetServerForUserAsync_ShouldReturnCorrectLookupStatusState()
    {
        var member = User.Create("Member", "m@chat.com", "pass", (u, p) => "hash");
        var outsider = User.Create("Outsider", "o@chat.com", "pass", (u, p) => "hash");
        Context.Users.AddRange(member, outsider);
        await Context.SaveChangesAsync();

        var server = ChatServer.CreateWithDefaults("Secure Server", member.Id);
        Context.Servers.Add(server);
        await Context.SaveChangesAsync();

        var (statusSuccess, data) = await _sut.GetServerForUserAsync(server.Id, member.Id, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(statusSuccess, Is.EqualTo(ServerLookupStatus.Success));
            Assert.That(data, Is.Not.Null);
            Assert.That(data!.Name, Is.EqualTo("Secure Server"));
        });

        var (statusForbidden, dataNull) = await _sut.GetServerForUserAsync(server.Id, outsider.Id, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(statusForbidden, Is.EqualTo(ServerLookupStatus.Forbidden));
            Assert.That(dataNull, Is.Null);
        });

        var (statusNotFound, _) = await _sut.GetServerForUserAsync(999, member.Id, CancellationToken.None);
        Assert.That(statusNotFound, Is.EqualTo(ServerLookupStatus.NotFound));
    }

    [Test]
    public async Task GetUserJoinedServersAsync_ShouldReturnOnlyServersTheUserHasJoined()
    {
        var user = User.Create("User", "u@chat.com", "pass", (u, p) => "hash");
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var server1 = ChatServer.CreateWithDefaults("Server 1", user.Id);
        var server2 = ChatServer.CreateWithDefaults("Server 2", user.Id);
        Context.Servers.AddRange(server1, server2);
        await Context.SaveChangesAsync();

        var alternateOwner = User.Create("Other", "other@chat.com", "pass", (u, p) => "hash");
        Context.Users.Add(alternateOwner);
        await Context.SaveChangesAsync();

        var unjoinedServer = ChatServer.CreateWithDefaults("Server 3", alternateOwner.Id);
        Context.Servers.Add(unjoinedServer);
        await Context.SaveChangesAsync();

        var joinedServers = await _sut.GetUserJoinedServersAsync(user.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(joinedServers, Has.Count.EqualTo(2));
            Assert.That(joinedServers.Any(s => s.Id == server1.Id), Is.True);
            Assert.That(joinedServers.Any(s => s.Id == server2.Id), Is.True);
            Assert.That(joinedServers.Any(s => s.Id == unjoinedServer.Id), Is.False, "Should filter out unjoined servers.");
        });
    }
}