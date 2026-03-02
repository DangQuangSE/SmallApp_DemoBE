using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SecondBike.Application.Interfaces;
using SecondBike.Domain.Common;
using SecondBike.Infrastructure.Data;

namespace SecondBike.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// Provides standard CRUD operations for all domain entities.
/// </summary>
/// <typeparam name="T">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> FindWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdWithIncludesAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }
}
