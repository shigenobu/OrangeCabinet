namespace OrangeCabinet;

/// <summary>
///     Lock.
/// </summary>
public class OcLock
{
    /// <summary>
    ///     Semaphore.
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    ///     Lock async.
    /// </summary>
    /// <returns>task</returns>
    public async Task<IDisposable> LockAsync()
    {
        await _semaphore.WaitAsync();
        return new OcLockHandler(_semaphore);
    }

    /// <summary>
    ///     Lock handler.
    /// </summary>
    private sealed class OcLockHandler : IDisposable
    {
        /// <summary>
        ///     Semaphore.
        /// </summary>
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        ///     Disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="semaphore">semaphore</param>
        public OcLockHandler(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _semaphore.Release();
            _disposed = true;
        }
    }
}