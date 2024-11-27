using System.Net;

namespace OrangeCabinet;

/// <summary>
///     Local for server.
/// </summary>
public class OcLocal
{
    /// <summary>
    ///     binder.
    /// </summary>
    private readonly OcBinder _binder;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="binder">binder</param>
    public OcLocal(OcBinder binder)
    {
        _binder = binder;
    }

    /// <summary>
    ///     Start.
    /// </summary>
    public void Start()
    {
        _binder.Bind(OcBindMode.Server);
    }

    /// <summary>
    ///     Send string to remote.
    ///     Enable to send message for some endpoint directly what you hope.
    ///     Notice, this method is not checked for remote endpoint state.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="remoteEndpoint">remote endpoint</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcLocalSendException">send error</exception>
    [Obsolete("Use async methods instead.")]
    public void SendTo(string message, IPEndPoint remoteEndpoint, int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        SendTo(message.OxToBytes(), remoteEndpoint, timeout);
    }

    /// <summary>
    ///     Async send string to remote.
    ///     Enable to send message for some endpoint directly what you hope.
    ///     Notice, this method is not checked for remote endpoint state.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="remoteEndpoint">remote endpoint</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcLocalSendException">send error</exception>
    public async Task SendToAsync(string message, IPEndPoint remoteEndpoint,
        int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        await SendToAsync(message.OxToBytes(), remoteEndpoint, timeout);
    }

    /// <summary>
    ///     Send bytes to remote.
    ///     Enable to send message for some endpoint directly what you hope.
    ///     Notice, this method is not checked for remote endpoint state.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="remoteEndpoint">remote endpoint</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcLocalSendException">send error</exception>
    [Obsolete("Use async methods instead.")]
    public void SendTo(byte[] message, IPEndPoint remoteEndpoint, int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        SendToAsync(message, remoteEndpoint, timeout).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Async send bytes to remote.
    ///     Enable to send message for some endpoint directly what you hope.
    ///     Notice, this method is not checked for remote endpoint state.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="remoteEndpoint">remote endpoint</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcLocalSendException">send error</exception>
    public async Task SendToAsync(byte[] message, IPEndPoint remoteEndpoint,
        int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        try
        {
            await _binder.SendToAsync(message, remoteEndpoint, timeout);
        }
        catch (Exception e)
        {
            OcLogger.Error(e);
            throw new OcLocalSendException(e);
        }
    }

    /// <summary>
    ///     Wait for.
    /// </summary>
    public void WaitFor()
    {
        _binder.WaitFor();
    }

    /// <summary>
    ///     Shutdown.
    /// </summary>
    public void Shutdown()
    {
        _binder.Close();
    }
}

/// <summary>
///     Local send exception.
/// </summary>
public class OcLocalSendException : Exception
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="e">exception</param>
    internal OcLocalSendException(Exception e) : base(e.ToString())
    {
    }
}