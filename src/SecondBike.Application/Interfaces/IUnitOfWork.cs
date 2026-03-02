namespace SecondBike.Application.Interfaces;

/// <summary>
/// Unit of Work pattern interface. Coordinates writing out changes
/// across multiple repositories in a single database transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Commits all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
