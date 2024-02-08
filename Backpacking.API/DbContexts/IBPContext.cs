using Backpacking.API.Models;
using Backpacking.API.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Backpacking.API.DbContexts;

public interface IBPContext
{
    DbSet<TEntity> GetSet<TEntity>() where TEntity : class, IBPModel;

    DbSet<Location> Locations { get; }

    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;
    ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class;
    EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

    EntityEntry Add(object entity);
    ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default(CancellationToken));
    EntityEntry Attach(object entity);
    EntityEntry Update(object entity);
    EntityEntry Remove(object entity);

    void AddRange(params object[] entities);
    Task AddRangeAsync(params object[] entities);
    void AttachRange(params object[] entities);
    void UpdateRange(params object[] entities);
    void RemoveRange(params object[] entities);

    void AddRange(IEnumerable<object> entities);
    Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default(CancellationToken));
    void AttachRange(IEnumerable<object> entities);
    void UpdateRange(IEnumerable<object> entities);
    void RemoveRange(IEnumerable<object> entities);


    object? Find(Type entityType, params object?[]? keyValues);
    ValueTask<object?> FindAsync(Type entityType, params object?[]? keyValues);
    ValueTask<object?> FindAsync(Type entityType, object?[]? keyValues, CancellationToken cancellationToken);
    TEntity? Find<TEntity>(params object?[]? keyValues) where TEntity : class;
    ValueTask<TEntity?> FindAsync<TEntity>(params object?[]? keyValues) where TEntity : class;
    ValueTask<TEntity?> FindAsync<TEntity>(object?[]? keyValues, CancellationToken cancellationToken) where TEntity : class;
    IQueryable<TResult> FromExpression<TResult>(Expression<Func<IQueryable<TResult>>> expression);

}
