using System.Reflection;
using System.Runtime.CompilerServices;

namespace OrangeCabinet;

/// <summary>
///     Callback.
/// </summary>
public abstract class OcCallback
{
    /// <summary>
    ///     Synchronous method names.
    /// </summary>
    internal static readonly List<string> SynchronousMethodNames = new() {"Incoming", "Timeout", "Shutdown"};

    /// <summary>
    ///     Use async callback.
    /// </summary>
    public virtual bool UseAsyncCallback { get; init; }

    /// <summary>
    ///     Contains async.
    /// </summary>
    /// <param name="callback">callback</param>
    /// <returns>if contains, return true</returns>
    internal static bool ContainsAsync(OcCallback callback)
    {
        var attType = typeof(AsyncStateMachineAttribute);
        foreach (var methodInfo in callback.GetType().GetMethods())
        {
            if (!SynchronousMethodNames.Contains(methodInfo.Name)) continue;

            var attrib = methodInfo.GetCustomAttribute(attType);
            if (attrib != null) return true;
        }

        return false;
    }

    /// <summary>
    ///     Incoming.
    /// </summary>
    /// <param name="remote">received remote</param>
    /// <param name="message">message</param>
    [Obsolete("Use async methods instead with 'UseAsyncCallback' setting to 'true'.")]
    public virtual void Incoming(OcRemote remote, byte[] message)
    {
    }

    /// <summary>
    ///     Async incoming.
    /// </summary>
    /// <param name="remote">received remote</param>
    /// <param name="message">message</param>
    /// <returns>task</returns>
    public virtual Task IncomingAsync(OcRemote remote, byte[] message)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Timeout.
    /// </summary>
    /// <param name="remote">be timeout remote</param>
    [Obsolete("Use async methods instead with 'UseAsyncCallback' setting to 'true'.")]
    public virtual void Timeout(OcRemote remote)
    {
    }

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
    ///     Shutdown.
    /// </summary>
    /// <param name="remote">be shutdown remote</param>
    [Obsolete("Use async methods instead with 'UseAsyncCallback' setting to 'true'.")]
    public virtual void Shutdown(OcRemote remote)
    {
    }

    /// <summary>
    ///     Shutdown.
    /// </summary>
    /// <param name="remote">be shutdown remote</param>
    /// <returns>task</returns>
    public virtual Task ShutdownAsync(OcRemote remote)
    {
        return Task.CompletedTask;
    }
}