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
    public void SendTo(string message, IPEndPoint remoteEndpoint)
    {
        _binder.SendTo(message.OxToBytes(), remoteEndpoint);
    }

    /// <summary>
    ///     Send bytes to remote.
    ///     Enable to send message for some endpoint directly what you hope.
    ///     Notice, this method is not checked for remote endpoint state.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="remoteEndpoint">remote endpoint</param>
    public void SendTo(byte[] message, IPEndPoint remoteEndpoint)
    {
        _binder.SendTo(message, remoteEndpoint);
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