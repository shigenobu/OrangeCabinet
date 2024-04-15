using System.Net;

namespace OrangeCabinet;

/// <summary>
///     Remote.
/// </summary>
public class OcRemote
{
    /// <summary>
    ///     Binder.
    /// </summary>
    private readonly OcBinder _binder;

    /// <summary>
    ///     Remote id.
    /// </summary>
    private readonly string _rid;

    /// <summary>
    ///     Idle milli seconds.
    /// </summary>
    private int _idleMilliSeconds = 10000;

    /// <summary>
    ///     Life timestamp milli seconds.
    /// </summary>
    private long _lifeTimestampMilliseconds;

    /// <summary>
    ///     Newest.
    /// </summary>
    private bool _newest = true;

    /// <summary>
    ///     Session values.
    /// </summary>
    private Dictionary<string, object>? _values;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="binder">binder</param>
    /// <param name="remoteHost">remote host</param>
    /// <param name="remotePort">remote port</param>
    public OcRemote(OcBinder binder, string remoteHost, int remotePort)
        : this(binder, new IPEndPoint(IPAddress.Parse(remoteHost), remotePort))
    {
    }

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="binder">binder</param>
    /// <param name="remoteEndpoint">remote endpoint</param>
    public OcRemote(OcBinder binder, IPEndPoint remoteEndpoint)
    {
        // bind
        _binder = binder;
        _binder.Bind(OcBindMode.Client);
        LocalEndpoint = (IPEndPoint) _binder.BindSocket!.OxSocketLocalEndPoint()!;
        RemoteEndpoint = remoteEndpoint;

        _rid = OcUtils.RandomString(16);
        _lifeTimestampMilliseconds = OcDate.NowTimestampMilliSeconds() + _idleMilliSeconds;
    }

    /// <summary>
    ///     Local endpoint.
    /// </summary>
    public IPEndPoint LocalEndpoint { get; }

    /// <summary>
    ///     Remote endpoint.
    /// </summary>
    public IPEndPoint RemoteEndpoint { get; }

    /// <summary>
    ///     Active.
    /// </summary>
    internal bool Active { get; set; } = true;

    /// <summary>
    ///     Lock.
    /// </summary>
    internal OcLock Lock { get; } = new();

    /// <summary>
    ///     Change idle milli seconds.
    /// </summary>
    /// <param name="idleMilliSeconds">idle milli seconds</param>
    public void ChangeIdleMilliSeconds(int idleMilliSeconds)
    {
        _idleMilliSeconds = idleMilliSeconds;
        UpdateTimeout();
    }

    /// <summary>
    ///     Is timeout.
    /// </summary>
    /// <returns>if timeout, return true</returns>
    internal bool IsTimeout()
    {
        return !_newest && OcDate.NowTimestampMilliSeconds() > _lifeTimestampMilliseconds;
    }

    /// <summary>
    ///     Update timeout.
    /// </summary>
    internal void UpdateTimeout()
    {
        _newest = false;
        _lifeTimestampMilliseconds = OcDate.NowTimestampMilliSeconds() + _idleMilliSeconds;
    }

    /// <summary>
    ///     Send string.
    ///     If remote is timeout or inactive, not send and throws exception.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcRemoteSendException">send error</exception>
    public void Send(string message, int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        Send(message.OxToBytes(), timeout);
    }

    /// <summary>
    ///     Async send string.
    ///     If remote is timeout or inactive, not send and throws exception.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcRemoteSendException">send error</exception>
    public async Task SendAsync(string message, int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        await SendAsync(message.OxToBytes(), timeout);
    }

    /// <summary>
    ///     Send bytes.
    ///     If remote is timeout or inactive, not send and throws exception.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcRemoteSendException">send error</exception>
    public void Send(byte[] message, int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        SendAsync(message, timeout).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Async send bytes.
    ///     If remote is timeout or inactive, not send and throws exception.
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="timeout">timeout</param>
    /// <exception cref="OcRemoteSendException">send error</exception>
    public async Task SendAsync(byte[] message, int timeout = OcBinder.DefaultTimeoutMilliSeconds)
    {
        // if escaped, disallow send
        if (_lifeTimestampMilliseconds <= 0) throw new OcRemoteSendException($"Remote({this}) is already escaped");

        // if not active, disallow send
        if (!Active) throw new OcRemoteSendException($"Remote({this}) is not active");

        try
        {
            await _binder.SendToAsync(message, RemoteEndpoint, timeout);
        }
        catch (Exception e)
        {
            OcLogger.Error(e);
            throw new OcRemoteSendException(e);
        }
    }

    /// <summary>
    ///     Escape.
    ///     It's force to timeout.
    /// </summary>
    public void Escape()
    {
        // lifetime is force to set 0
        _lifeTimestampMilliseconds = 0;
        OcLogger.Debug(() => $"Escape {this}");
    }

    /// <summary>
    ///     Set value.
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    public void SetValue(string name, object value)
    {
        _values ??= new Dictionary<string, object>();
        _values[name] = value;
    }

    /// <summary>
    ///     Get value.
    /// </summary>
    /// <param name="name">name</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>value or null</returns>
    public T? GetValue<T>(string name)
    {
        if (_values == null) return default;
        if (!_values.ContainsKey(name)) return default;
        return (T?) _values[name];
    }

    /// <summary>
    ///     Clear value.
    /// </summary>
    /// <param name="name">name</param>
    public void ClearValue(string name)
    {
        _values?.Remove(name);
    }

    /// <summary>
    ///     To string.
    /// </summary>
    /// <returns>remote id</returns>
    public override string ToString()
    {
        return $"Rid:{_rid}, " +
               $"Local:{LocalEndpoint.OxToHostPort()}, " +
               $"Remote:{RemoteEndpoint.OxToHostPort()}";
    }
}

/// <summary>
///     Remote send exception.
/// </summary>
public class OcRemoteSendException : Exception
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="e">exception</param>
    internal OcRemoteSendException(Exception e) : base(e.ToString())
    {
    }

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="message">message</param>
    internal OcRemoteSendException(string message) : base(message)
    {
    }
}