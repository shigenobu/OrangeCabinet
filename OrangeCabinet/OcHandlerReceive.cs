using System.Net;
using System.Net.Sockets;

namespace OrangeCabinet;

/// <summary>
///     Handler receive.
/// </summary>
internal class OcHandlerReceive : OcHandler<OcStateReceive>
{
    /// <summary>
    ///     Callback.
    /// </summary>
    private readonly OcCallback _callback;

    /// <summary>
    ///     Read buffer size.
    /// </summary>
    private readonly int _readBufferSize;

    /// <summary>
    ///     Reset event for receive.
    /// </summary>
    private readonly ManualResetEventSlim _received = new(false);

    /// <summary>
    ///     Remote manager.
    /// </summary>
    private readonly OcRemoteManager _remoteManager;

    /// <summary>
    ///     Cancellation token for receive task.
    /// </summary>
    private readonly CancellationTokenSource _tokenSourceReceive;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="callback">callback</param>
    /// <param name="readBufferSize">read buffer size</param>
    /// <param name="remoteManager">remote manager</param>
    internal OcHandlerReceive(OcCallback callback, int readBufferSize, OcRemoteManager remoteManager)
    {
        _callback = callback;
        _readBufferSize = readBufferSize;
        _remoteManager = remoteManager;

        _tokenSourceReceive = new CancellationTokenSource();
    }

    /// <summary>
    ///     Receive task.
    /// </summary>
    internal Task? TaskReceive { get; private set; }

    /// <summary>
    ///     Prepare.
    /// </summary>
    /// <param name="state">state</param>
    internal override void Prepare(OcStateReceive state)
    {
        TaskReceive = Task.Factory.StartNew(() =>
        {
            while (true)
            {
                // check cancel
                if (_tokenSourceReceive.Token.IsCancellationRequested)
                {
                    OcLogger.Info($"Cancel receive task: {_tokenSourceReceive.Token.GetHashCode()}");
                    return;
                }

                // signal off
                _received.Reset();

                try
                {
                    // reset
                    // Must be reset every time.
                    state = new OcStateReceive
                    {
                        Socket = state.Socket,
                        Buffer = new byte[_readBufferSize]
                    };

                    // receive
                    // 'BeginReceiveFrom' and 'EndReceiveFrom' must be used at same time.
                    EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    state.Socket.BeginReceiveFrom(
                        state.Buffer!,
                        0,
                        state.Buffer!.Length,
                        SocketFlags.None,
                        ref remoteEndpoint,
                        Complete,
                        state);
                }
                catch (Exception e)
                {
                    OcLogger.Debug(() => e);
                    Failed(state);
                }

                // wait until signal on
                _received.Wait();
            }
        }, _tokenSourceReceive.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    /// <summary>
    ///     Complete.
    /// </summary>
    /// <param name="result">async result</param>
    internal override void Complete(IAsyncResult result)
    {
        // signal on
        _received.Set();

        // get state
        if (!GetState(result, out var state))
        {
            OcLogger.Debug(() => $"When received, no state result: {result}");
            return;
        }

        EndPoint? remoteEndpoint;
        int received;
        try
        {
            // received
            remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            received = state!.Socket.EndReceiveFrom(result, ref remoteEndpoint);
            if (received <= 0)
            {
                OcLogger.Debug(() => $"Received wrong size: {received}");
                return;
            }
        }
        catch (Exception e)
        {
            OcLogger.Debug(() => e);
            Failed(state!);
            return;
        }

        // callback
        var taskReceive = Task.Run(async () =>
        {
            var remote = await _remoteManager.GenerateAsync((IPEndPoint) remoteEndpoint);
            OcLogger.Debug(() => $"Received remote: {remote}, size: {received}");
            using (await remote.Lock.LockAsync())
            {
                // if remote is active and not timeout, invoke incoming
                if (remote.Active && !remote.IsTimeout())
                {
                    var message = new byte[received];
                    Buffer.BlockCopy(state.Buffer!, 0, message, 0, message.Length);
                    remote.UpdateTimeout();
                    await _callback.IncomingAsync(remote, message);
                }
            }
        });
        taskReceive.ContinueWith(comp =>
        {
            if (comp.Exception is not { } e) return;
            OcLogger.Debug(() => e.InnerExceptions);
            Failed(state);
        });
    }

    /// <summary>
    ///     Failed.
    /// </summary>
    /// <param name="state">state</param>
    internal override void Failed(OcStateReceive state)
    {
        OcLogger.Debug(() => $"Receive failed: {state}");
    }

    /// <summary>
    ///     Shutdown.
    /// </summary>
    internal override void Shutdown()
    {
        // shutdown receive
        if (TaskReceive is {IsCanceled: false}) _tokenSourceReceive.Cancel();
    }
}