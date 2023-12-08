using System.Net;
using System.Net.Sockets;

namespace OrangeCabinet;

/// <summary>
///     Binder.
/// </summary>
public class OcBinder : IDisposable
{
    /// <summary>
    ///     Receive handler.
    /// </summary>
    private OcHandlerReceive? _handlerReceive;

    /// <summary>
    ///     Remote manager.
    /// </summary>
    private OcRemoteManager? _remoteManager;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="callback">callback</param>
    public OcBinder(OcCallback callback)
    {
        Callback = callback;
    }

    /// <summary>
    ///     Callback.
    /// </summary>
    internal OcCallback Callback { get; }

    /// <summary>
    ///     Bind host, default 0.0.0.0.
    /// </summary>
    public string BindHost { get; init; } = "0.0.0.0";

    /// <summary>
    ///     Bind port, default random between 18000-28999.
    /// </summary>
    public int BindPort { get; init; } = OcUtils.RandomInt(18000, 27999);

    /// <summary>
    ///     ReadBufferSize for read(receive).
    /// </summary>
    public int ReadBufferSize { get; init; } = 1350;

    /// <summary>
    ///     Divide.
    ///     It's remote divided number.
    /// </summary>
    public int Divide { get; set; } = 10;

    /// <summary>
    ///     Bind socket.
    /// </summary>
    internal Socket? BindSocket { get; private set; }

    /// <summary>
    ///     Dispose.
    /// </summary>
    public void Dispose()
    {
        Close();
    }

    /// <summary>
    ///     Bind.
    /// </summary>
    /// <param name="bindMode">bind mode</param>
    /// <exception cref="OcBinderException">bind exception</exception>
    internal void Bind(OcBindMode bindMode)
    {
        if (BindSocket != null) return;

        // if client, force divide 1
        if (bindMode == OcBindMode.Client) Divide = 1;

        try
        {
            // init
            BindSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            BindSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            BindSocket.Bind(new IPEndPoint(IPAddress.Parse(BindHost), BindPort));
            BindSocket.Blocking = false;

            // manager
            _remoteManager = new OcRemoteManager(this);
            _remoteManager.StartTimeoutTask();

            // handler
            _handlerReceive = new OcHandlerReceive(Callback, ReadBufferSize, _remoteManager);
            var state = new OcStateReceive
            {
                Socket = BindSocket
            };
            _handlerReceive.Prepare(state);

            // start
            OcLogger.Info($"{bindMode} bind on {BindHost}:{BindPort} (readBufferSize:{ReadBufferSize})");
        }
        catch (Exception e)
        {
            OcLogger.Error(e);
            throw new OcBinderException(e);
        }
    }

    /// <summary>
    ///     Send bytes to remote.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="remoteEndpoint">remote endpoint</param>
    /// <exception cref="OcBinderException">bind exception</exception>
    internal void SendTo(byte[] message, IPEndPoint remoteEndpoint)
    {
        if (BindSocket == null)
            throw new OcBinderException($"Not bind on {BindSocket}:{BindPort}");
        BindSocket.SendTo(message, SocketFlags.None, remoteEndpoint);
    }

    /// <summary>
    ///     Wait for.
    /// </summary>
    internal void WaitFor()
    {
        _handlerReceive?.TaskReceive?.Wait();
    }

    /// <summary>
    ///     Close.
    /// </summary>
    internal void Close()
    {
        // manager
        _remoteManager?.ShutdownTimeoutTask();

        // handler
        _handlerReceive?.Shutdown();

        // close
        BindSocket?.Close();
        BindSocket = null;
    }

    /// <summary>
    ///     Get remote count.
    /// </summary>
    /// <returns>remote count</returns>
    public long GetRemoteCount()
    {
        return _remoteManager?.GetRemoteCount() ?? 0;
    }

    /// <summary>
    ///     To string.
    /// </summary>
    /// <returns>local host and port, or empty.</returns>
    public override string ToString()
    {
        return $"Bind socket: {BindSocket?.OxSocketLocalEndPoint()}";
    }
}

/// <summary>
///     Binder mode.
/// </summary>
internal enum OcBindMode
{
    /// <summary>
    ///     Server.
    /// </summary>
    Server,

    /// <summary>
    ///     Client.
    /// </summary>
    Client
}

/// <summary>
///     Bind exception.
/// </summary>
public class OcBinderException : Exception
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="e">exception</param>
    internal OcBinderException(Exception e) : base(e.ToString())
    {
    }

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="message">message</param>
    internal OcBinderException(string message) : base(message)
    {
    }
}