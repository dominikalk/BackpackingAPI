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
    public IQueryable<Location> VisitedLocations => Locations.Where(location => location.LocationType == LocationType.VisitedLocation);
    public IQueryable<Location> PlannedLocations => Locations.Where(location => location.LocationType == LocationType.PlannedLocation);

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
            if (modifiedEntry is IBPModel applicationModel)
            {
                applicationModel.LastModifiedDate = DateTimeOffset.UtcNow;
            }
        }
    }
}

