using Backpacking.API.Models;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backpacking.API.DbContexts;

public class BPContext : IdentityDbContext<BPUser, IdentityRole<Guid>, Guid>, IBPContext
{
    private readonly ILogger<BPContext> _logger;

    public BPContext(
        DbContextOptions<BPContext> options,
        ILogger<BPContext> logger) : base(options)
    {
        _logger = logger;
    }

    public DbSet<TEntity> GetSet<TEntity>() where TEntity : class, IBPModel => Set<TEntity>();

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<UserRelation> UserRelations => Set<UserRelation>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRelation>().HasKey(r => new { r.SentById, r.SentToId });

        modelBuilder.Entity<UserRelation>()
            .HasOne<BPUser>(a => a.SentBy)
            .WithMany(b => b.SentUserRelations)
            .HasForeignKey(c => c.SentById);

        modelBuilder.Entity<UserRelation>()
            .HasOne<BPUser>(a => a.SentTo)
            .WithMany(b => b.ReceivedUserRelations)
            .HasForeignKey(c => c.SentToId);
    }

    public new async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            UpdateTimestamps();
            await base.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Save changes failed");
            return new BPError(HttpStatusCode.BadRequest, "Could not save changes.");
        }
    }

    private void UpdateTimestamps()
    {
        IEnumerable<object> insertedEntries = ChangeTracker.Entries()
               .Where(entityEntry => entityEntry.State == EntityState.Added)
               .Select(entityEntry => entityEntry.Entity);

        // add created date for new entries
        foreach (var insertedEntry in insertedEntries)
        {
            if (insertedEntry is IBPModel applicationModel)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                applicationModel.CreatedDate = now;
                applicationModel.LastModifiedDate = now;
            }
        }

        // update last modified date for existing entries
        IEnumerable<object> modifiedEntries = ChangeTracker.Entries()
               .Where(entityEntry => entityEntry.State == EntityState.Modified)
               .Select(entityEntry => entityEntry.Entity);

        foreach (var modifiedEntry in modifiedEntries)
        {
            if (modifiedEntry is IBPModel applicationModel)
            {
                applicationModel.LastModifiedDate = DateTimeOffset.UtcNow;
            }
        }
    }
}

