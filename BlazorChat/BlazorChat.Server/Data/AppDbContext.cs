using BlazorChat.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<ChatServer> Servers { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<ServerMembership> ServerMemberships { get; set; }
    public DbSet<DirectMessage> DirectMessages { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── User ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();

        // ── Friendship ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Friendship>()
            .HasKey(f => new { f.RequesterId, f.ReceiverId });

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany(u => u.SentRequests)
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Receiver)
            .WithMany(u => u.ReceivedRequests)
            .HasForeignKey(f => f.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── ChatServer ─────────────────────────────────────────────────────────
        modelBuilder.Entity<ChatServer>()
            .HasOne(s => s.Owner)
            .WithMany()
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // don't wipe server when owner account deleted

        // ── Channel ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Channel>()
            .HasOne(c => c.Server)
            .WithMany(s => s.Channels)
            .HasForeignKey(c => c.ServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Channel>()
            .HasIndex(c => new { c.ServerId, c.SortOrder });

        // ── ServerMembership ───────────────────────────────────────────────────
        modelBuilder.Entity<ServerMembership>()
            .HasKey(sm => new { sm.ServerId, sm.UserId });

        modelBuilder.Entity<ServerMembership>()
            .HasOne(sm => sm.Server)
            .WithMany(s => s.Members)
            .HasForeignKey(sm => sm.ServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServerMembership>()
            .HasOne(sm => sm.User)
            .WithMany(u => u.ServerMemberships)
            .HasForeignKey(sm => sm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── DirectMessage ──────────────────────────────────────────────────────
        modelBuilder.Entity<DirectMessage>()
            .HasOne(dm => dm.User1)
            .WithMany()
            .HasForeignKey(dm => dm.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DirectMessage>()
            .HasOne(dm => dm.User2)
            .WithMany()
            .HasForeignKey(dm => dm.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure only one DM conversation per pair, regardless of order
        modelBuilder.Entity<DirectMessage>()
            .HasIndex(dm => new { dm.User1Id, dm.User2Id }).IsUnique();

        // ── Message ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Author)
            .WithMany()
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // keep messages if user deleted

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Channel)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.DirectMessage)
            .WithMany(dm => dm.Messages)
            .HasForeignKey(m => m.DirectMessageId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Index for fast channel message paging (most common query)
        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.ChannelId, m.CreatedAt });

        // Index for DM message paging
        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.DirectMessageId, m.CreatedAt });
    }
}