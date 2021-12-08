using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OrangeCabinet
{
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
        
        public OcRemoteManager(OcBinder binder)
        {
            _binder = binder;
            
            _remoteLocks = new List<object>(_binder.Divide);
            for (int i = 0; i < _binder.Divide; i++)
            {
                _remoteLocks.Add(new object());
            }
            
            _remotes = new List<ConcurrentDictionary<string, OcRemote>>(_binder.Divide);
            for (int i = 0; i < _binder.Divide; i++)
            {
                _remotes.Add(new ConcurrentDictionary<string, OcRemote>());
            }
        }

        internal void StartTimeoutTask()
        {
            var delay = 1000 / _binder.Divide;
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
                    if (_taskNo >= _binder.Divide) _taskNo = 0;

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
            OcLogger.Info($"Closing connections at shutdown");
            for (int i = 0; i < _binder.Divide; i++)
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
        
        private int GetMod(string hostPort)
        {
            return Math.Abs(hostPort.GetHashCode() % _binder.Divide);
        }
        
        private bool TryGet(string hostPort, out OcRemote? remote)
        {
            int mod = GetMod(hostPort);
            return _remotes[mod].TryGetValue(hostPort, out remote);
        }
        
        public OcRemote Generate(IPEndPoint remoteEndpoint)
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