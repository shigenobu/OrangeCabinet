namespace OrangeCabinet;

/// <summary>
///     Callback.
/// </summary>
public abstract class OcCallback
{
    /// <summary>
    ///     Async incoming.
    /// </summary>
    /// <param name="remote">received remote</param>
    /// <param name="message">message</param>
    /// <returns>task</returns>
    public abstract Task IncomingAsync(OcRemote remote, byte[] message);

    /// <summary>
    ///     Async timeout.
    /// </summary>
    /// <param name="remote">be timeout remote</param>
    /// <returns>task</returns>
    public virtual Task TimeoutAsync(OcRemote remote)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Async shutdown.
    /// </summary>
    /// <param name="remote">be shutdown remote</param>
    /// <returns>task</returns>
    public virtual Task ShutdownAsync(OcRemote remote)
    {
        return Task.CompletedTask;
    }
}