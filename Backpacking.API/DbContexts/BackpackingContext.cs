using Backpacking.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backpacking.API.DbContexts;

public class BackpackingContext : IdentityDbContext<User>, IBackpackingContext
{
    private readonly ILogger<BackpackingContext> _logger;

    public BackpackingContext(
        DbContextOptions<BackpackingContext> options,
        ILogger<BackpackingContext> logger) : base(options)
    {
        _logger = logger;
    }

    public DbSet<TEntity> GetSet<TEntity>() where TEntity : class, IModel => Set<TEntity>();

    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>();

        base.OnModelCreating(modelBuilder);
    }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            UpdateTimestamps();
            await base.SaveChangesAsync(cancellationToken);

            return (int)HttpStatusCode.OK;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Save changes failed");
            return (int)HttpStatusCode.InternalServerError;
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
            if (insertedEntry is IModel applicationModel)
            {
                applicationModel.CreatedDate = DateTimeOffset.UtcNow;
                applicationModel.LastModifiedDate = DateTimeOffset.UtcNow;
            }
        }

        // update last modified date for existing entries
        IEnumerable<object> modifiedEntries = ChangeTracker.Entries()
               .Where(entityEntry => entityEntry.State == EntityState.Modified)
               .Select(entityEntry => entityEntry.Entity);

        foreach (var modifiedEntry in modifiedEntries)
        {
            if (modifiedEntry is IModel applicationModel)
            {
                applicationModel.LastModifiedDate = DateTimeOffset.UtcNow;
            }
        }
    }
}

