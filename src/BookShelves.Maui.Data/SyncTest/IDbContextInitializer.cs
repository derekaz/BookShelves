namespace BookShelves.Maui.Data.SyncTest;

/// <summary>
/// An interface to initialize a database.
/// </summary>
public interface IDbInitializer
{
    /// <summary>
    /// Synchronously initialize the database.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Asynchronously initialize the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that resolves when complete.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
