using System.Collections.Concurrent;
using System.Net;

namespace OrangeCabinet;

/// <summary>
///     Remote manager.
/// </summary>
public class OcRemoteManager
{
    /// <summary>
    ///     Binder.
    /// </summary>
    private readonly OcBinder _binder;

    /// <summary>
    ///     Divide.
    /// </summary>
    private readonly int _divide;

    /// <summary>
    ///     Remote locks.
    /// </summary>
    private readonly List<OcLock> _remoteLocks;

    /// <summary>
    ///     Remotes.
    /// </summary>
    private readonly List<ConcurrentDictionary<string, OcRemote>> _remotes;

    /// <summary>
    ///     Remote count.
    /// </summary>
    private long _remoteCount;

    /// <summary>
    ///     Timeout task.
    /// </summary>
    private Task? _taskTimeout;

    /// <summary>
    ///     Cancellation token for timeout task.
    /// </summary>
    private CancellationTokenSource? _tokenSourceTimeout;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="binder">binder</param>
    internal OcRemoteManager(OcBinder binder)
    {
        _binder = binder;
        _divide = binder.Divide;

        _remoteLocks = new List<OcLock>(_divide);
        for (var i = 0; i < _divide; i++) _remoteLocks.Add(new OcLock());

        _remotes = new List<ConcurrentDictionary<string, OcRemote>>(_divide);
        for (var i = 0; i < _divide; i++) _remotes.Add(new ConcurrentDictionary<string, OcRemote>());
    }

    /// <summary>
    ///     Start timeout task.
    /// </summary>
    internal void StartTimeoutTask()
    {
        var delay = 1000 / _divide;
        _tokenSourceTimeout = new CancellationTokenSource();
        _taskTimeout = Task.Factory.StartNew(async () =>
        {
            var taskNo = 0;
            while (true)
            {
                // check cancel
                if (_tokenSourceTimeout.Token.IsCancellationRequested)
                {
                    OcLogger.Info($"Cancel timeout task: {_tokenSourceTimeout.Token.GetHashCode()}");
                    return;
                }

                // delay
                await Task.Delay(delay);

                // increment task no
                taskNo++;
                if (taskNo >= _divide) taskNo = 0;

                // timeout
                using (await _remoteLocks[taskNo].LockAsync())
                {
                    foreach (var pair in _remotes[taskNo])
                        using (await pair.Value.Lock.LockAsync())
                        {
                            // if already timeout and active, invoke timeout.
                            if (pair.Value.Active && pair.Value.IsTimeout())
                            {
                                pair.Value.Active = false;
                                if (_binder.Callback.UseAsyncCallback)
                                    await _binder.Callback.TimeoutAsync(pair.Value);
                                else
                                    // ReSharper disable once MethodHasAsyncOverload
                                    _binder.Callback.Timeout(pair.Value);

                                if (_remotes[taskNo].TryRemove(pair))
                                {
                                    // decrement
                                    Interlocked.Decrement(ref _remoteCount);
                                    OcLogger.Debug(() => $"By timeout, removed remote: {pair.Value}");
                                }
                            }
                        }
                }
            }
        }, _tokenSourceTimeout.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    /// <summary>
    ///     Shutdown timeout task.
    /// </summary>
    internal void ShutdownTimeoutTask()
    {
        if (_taskTimeout == null) return;
        if (_tokenSourceTimeout == null) return;
        if (_taskTimeout.IsCanceled) return;

        // cancel
        _tokenSourceTimeout.Cancel();

        // shutdown all sessions
        OcLogger.Info("Closing remotes at shutdown");
        Task.Run(async () =>
        {
            for (var i = 0; i < _divide; i++)
                using (await _remoteLocks[i].LockAsync())
                {
                    foreach (var pair in _remotes[i])
                        using (await pair.Value.Lock.LockAsync())
                        {
                            // if active, invoke shutdown.
                            if (pair.Value.Active)
                            {
                                pair.Value.Active = false;
                                if (_binder.Callback.UseAsyncCallback)
                                    await _binder.Callback.ShutdownAsync(pair.Value);
                                else
                                    // ReSharper disable once MethodHasAsyncOverload
                                    _binder.Callback.Shutdown(pair.Value);

                                if (_remotes[i].TryRemove(pair))
                                {
                                    // decrement
                                    Interlocked.Decrement(ref _remoteCount);
                                    OcLogger.Debug(() => $"By shutdown, removed remote: {pair.Value}");
                                }
                            }
                        }
                }
        });
    }

    /// <summary>
    ///     Get mod.
    /// </summary>
    /// <param name="hostPort">host and port string</param>
    /// <returns>mod</returns>
    private int GetMod(string hostPort)
    {
        return Math.Abs(hostPort.GetHashCode() % _divide);
    }

    /// <summary>
    ///     Try to get remote.
    /// </summary>
    /// <param name="hostPort">host and port string</param>
    /// <param name="remote">remote</param>
    /// <returns>if to get, return true</returns>
    private bool TryGet(string hostPort, out OcRemote? remote)
    {
        var mod = GetMod(hostPort);
        return _remotes[mod].TryGetValue(hostPort, out remote);
    }

    /// <summary>
    ///     Async generate.
    /// </summary>
    /// <param name="remoteEndpoint">remote endpoint</param>
    /// <returns>remote</returns>
    internal async Task<OcRemote> GenerateAsync(IPEndPoint remoteEndpoint)
    {
        var hostPort = remoteEndpoint.OxToHostPort();
        var mod = GetMod(hostPort);

        OcRemote? remote;
        using (await _remoteLocks[mod].LockAsync())
        {
            if (!TryGet(hostPort, out remote))
            {
                var tmpRemote = new OcRemote(_binder, remoteEndpoint);
                remote = _remotes[mod].GetOrAdd(hostPort, tmpRemote);
                if (tmpRemote == remote)
                {
                    Interlocked.Increment(ref _remoteCount);
                    OcLogger.Debug(() => $"Generate remote: {remote}");
                }
            }
        }

        return remote!;
    }

    /// <summary>
    ///     Get remote count.
    /// </summary>
    /// <returns>remote count</returns>
    public long GetRemoteCount()
    {
        return Interlocked.Read(ref _remoteCount);
    }
}