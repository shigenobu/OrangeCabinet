using System;
using System.Net;
using System.Net.Sockets;

namespace OrangeCabinet
{
    /// <summary>
    ///     Binder.
    /// </summary>
    public class OcBinder : IDisposable
    {
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
        public int Divide { get; init; } = 10;
        
        /// <summary>
        ///     Receive handler.
        /// </summary>
        private OcHandlerReceive? _handlerReceive;

        /// <summary>
        ///     Remote manager.
        /// </summary>
        private OcRemoteManager? _remoteManager;

        /// <summary>
        ///     Bind socket.
        /// </summary>
        internal Socket? BindSocket { get; private set; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="callback">callback</param>
        public OcBinder(OcCallback callback)
        {
            Callback = callback;
        }

        /// <summary>
        ///     Bind.
        /// </summary>
        /// <param name="bindMode">bind mode</param>
        /// <exception cref="OcBinderException">bind exception</exception>
        internal void Bind(OcBindMode bindMode)
        {
            if (BindSocket != null)
            {
                return;
            }

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
                    Socket = BindSocket,
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
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Close();
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
        {}
    }
}