using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OrangeCabinet
{
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
        ///     Remote locks.
        /// </summary>
        private readonly List<object> _remoteLocks;
        
        /// <summary>
        ///     Remotes.
        /// </summary>
        private readonly List<ConcurrentDictionary<string, OcRemote>> _remotes;

        /// <summary>
        ///     Divide.
        /// </summary>
        private readonly int _divide;
        
        /// <summary>
        ///     Remote count.
        /// </summary>
        private long _remoteCount;
        
        /// <summary>
        ///     Cancellation token for timeout task.
        /// </summary>
        private CancellationTokenSource? _tokenSourceTimeout;
        
        /// <summary>
        ///     Timeout task.
        /// </summary>
        private Task? _taskTimeout;

        /// <summary>
        ///     Task no.
        /// </summary>
        private int _taskNo;
        
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="binder">binder</param>
        internal OcRemoteManager(OcBinder binder)
        {
            _binder = binder;
            _divide = binder.Divide;
            
            _remoteLocks = new List<object>(_divide);
            for (int i = 0; i < _divide; i++)
            {
                _remoteLocks.Add(new object());
            }
            
            _remotes = new List<ConcurrentDictionary<string, OcRemote>>(_divide);
            for (int i = 0; i < _divide; i++)
            {
                _remotes.Add(new ConcurrentDictionary<string, OcRemote>());
            }
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
                    _taskNo++;
                    if (_taskNo >= _divide) _taskNo = 0;

                    // timeout
                    lock (_remoteLocks[_taskNo])
                    {
                        foreach (var pair in _remotes[_taskNo])
                        {
                            lock (pair.Value)
                            {
                                // if already timeout and active, invoke timeout.
                                if (pair.Value.Active && pair.Value.IsTimeout())
                                {
                                    pair.Value.Active = false;
                                    _binder.Callback.Timeout(pair.Value);

                                    if (_remotes[_taskNo].TryRemove(pair))
                                    {
                                        // decrement
                                        Interlocked.Decrement(ref _remoteCount);
                                        OcLogger.Debug(() => $"By timeout, removed remote: {pair.Value}");
                                    }
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
            if (_taskTimeout.IsCanceled)
            {
                return;
            }
            
            // cancel
            _tokenSourceTimeout.Cancel();
            
            // shutdown all sessions
            OcLogger.Info($"Closing remotes at shutdown");
            for (int i = 0; i < _divide; i++)
            {
                lock (_remoteLocks[i])
                {
                    foreach (var pair in _remotes[i])
                    {
                        lock (pair.Value)
                        {
                            // if active, invoke shutdown.
                            if (pair.Value.Active)
                            {
                                pair.Value.Active = false;
                                _binder.Callback.Shutdown(pair.Value);

                                if (_remotes[_taskNo].TryRemove(pair))
                                {
                                    // decrement
                                    Interlocked.Decrement(ref _remoteCount);
                                    OcLogger.Debug(() => $"By shutdown, removed remote: {pair.Value}");
                                }
                            }
                        }
                    }
                }
            }
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
        ///     Try get remote.
        /// </summary>
        /// <param name="hostPort">host and port string</param>
        /// <param name="remote">remote</param>
        /// <returns>if get, return true</returns>
        private bool TryGet(string hostPort, out OcRemote? remote)
        {
            int mod = GetMod(hostPort);
            return _remotes[mod].TryGetValue(hostPort, out remote);
        }
        
        /// <summary>
        ///     Generate.
        /// </summary>
        /// <param name="remoteEndpoint">remote endpoint</param>
        /// <returns>remote</returns>
        internal OcRemote Generate(IPEndPoint remoteEndpoint)
        {
            string hostPort = remoteEndpoint.OxToHostPort();
            int mod = GetMod(hostPort);
            if (_remotes[mod].ContainsKey(hostPort))
            {
                return _remotes[mod][hostPort];
            }

            OcRemote? remote;
            lock (_remoteLocks[mod])
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
}