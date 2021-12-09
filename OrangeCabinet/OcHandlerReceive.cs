using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OrangeCabinet
{
    public class OcHandlerReceive : OcHandler<OcStateReceive>
    {
        /// <summary>
        ///     Reset event for accept.
        /// </summary>
        private readonly ManualResetEventSlim _received = new(false);

        /// <summary>
        ///     Callback.
        /// </summary>
        private readonly OcCallback _callback;

        /// <summary>
        ///     Read buffer size.
        /// </summary>
        private readonly int _readBufferSize;

        /// <summary>
        ///     Remote manager.
        /// </summary>
        private readonly OcRemoteManager _remoteManager;
        
        /// <summary>
        ///     Cancellation token for receive task.
        /// </summary>
        private readonly CancellationTokenSource _tokenSourceReceive;
        
        /// <summary>
        ///     Receive task.
        /// </summary>
        internal Task? TaskReceive { get; private set; }
        
        public OcHandlerReceive(OcCallback callback, int readBufferSize, OcRemoteManager remoteManager)
        {
            _callback = callback;
            _readBufferSize = readBufferSize;
            _remoteManager = remoteManager;
            
            _tokenSourceReceive = new CancellationTokenSource();
        }
        
        public override void Prepare(OcStateReceive state)
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
                        // reset buffer
                        if (state.Buffer == null)
                        {
                            state.Buffer = new byte[_readBufferSize];    
                        }
                        else
                        {
                            // new
                            state = new OcStateReceive()
                            {
                                Socket = state.Socket,
                                Buffer = new byte[_readBufferSize]
                            };
                        }
                        
                        
                        // receive
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

        public override void Complete(IAsyncResult result)
        {
            // signal on
            _received.Set();
            
            // get state
            if (!GetState(result, out var state))
            {
                OcLogger.Debug(() => $"When received, no state result: {result}");
                return;
            }
            
            try
            {
                // received
                EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                int received = state!.Socket.EndReceiveFrom(result, ref remoteEndpoint);
                if (received <= 0)
                {
                    OcLogger.Debug(() => $"Received wrong size: {received}");    
                    return;
                }
                var remote = _remoteManager.Generate((IPEndPoint)remoteEndpoint);
                OcLogger.Debug(() => $"Received remote: {remote}, size: {received}");
                lock (remote)
                {
                    // if remote is active and not timeout, invoke incoming
                    if (remote.Active && !remote.IsTimeout())
                    {
                        byte[] message = new byte[received];
                        Buffer.BlockCopy(state.Buffer!, 0, message, 0, message.Length);
                        remote.UpdateTimeout();
                        _callback.Incoming(remote, message);    
                    }
                }
            }
            catch (Exception e)
            {
                OcLogger.Debug(() => e);
                Failed(state!);
            }
        }

        public override void Failed(OcStateReceive state)
        {
            OcLogger.Debug(() => $"Receive failed: {state}");
        }

        public override void Shutdown()
        {
            // shutdown receive
            if (TaskReceive is { IsCanceled: false }) _tokenSourceReceive.Cancel();
        }
    }
}