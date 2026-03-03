using SecondBike.Application.Interfaces;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation that wraps the SecondBikeDbContext.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SecondBikeDbContext _context;

    public UnitOfWork(SecondBikeDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
