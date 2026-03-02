using SecondBike.Application.Interfaces;
using SecondBike.Infrastructure.Data;

namespace SecondBike.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation that wraps the AppDbContext
/// to coordinate saving changes across multiple repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Releases the database context resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        // AppDbContext is managed by the DI container.
        // We do NOT dispose it here to avoid ObjectDisposedException in other services
        // sharing the same context instance in the same scope (especially in Blazor).
        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
