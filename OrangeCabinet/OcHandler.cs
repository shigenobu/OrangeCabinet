namespace OrangeCabinet;

/// <summary>
///     Handler.
/// </summary>
/// <typeparam name="T">type of state</typeparam>
internal abstract class OcHandler<T> where T : OcState
{
    /// <summary>
    ///     Get state.
    /// </summary>
    /// <param name="result">async result</param>
    /// <param name="state">state</param>
    /// <returns>if cast is success, return true</returns>
    protected bool GetState(IAsyncResult result, out T? state)
    {
        state = default;
        if (result.AsyncState != null) state = (T) result.AsyncState;
        return state != null;
    }

    /// <summary>
    ///     Prepare (Receive)
    /// </summary>
    /// <param name="state">state</param>
    internal abstract void Prepare(T state);

    /// <summary>
    ///     Complete (receive)
    /// </summary>
    /// <param name="result"></param>
    internal abstract void Complete(IAsyncResult result);

    /// <summary>
    ///     Failed.
    /// </summary>
    /// <param name="state">state</param>
    internal abstract void Failed(T state);

    /// <summary>
    ///     Shutdown.
    /// </summary>
    internal abstract void Shutdown();
}