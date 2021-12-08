using System;
using System.Net;
using System.Net.Sockets;

namespace OrangeCabinet
{
    public class OcBinder
    {
        internal OcCallback Callback { get; }

        public string BindHost { get; init; } = "0.0.0.0";

        public int BindPort { get; init; } = OcUtils.RandomInt(18000, 27999);
        
        /// <summary>
        ///     ReadBufferSize for read(receive).
        /// </summary>
        public int ReadBufferSize { get; init; } = 1350;

        /// <summary>
        ///     Divide.
        ///     It's remote connections divided number.
        /// </summary>
        public int Divide { get; init; } = 10;
        
        private OcHandlerReceive _handlerReceive;

        private OcRemoteManager _remoteManager;

        internal Socket? BindSocket { get; private set; }

        public OcBinder(OcCallback callback)
        {
            Callback = callback;
        }
        
        internal void Bind()
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
                OcLogger.Info($"Bind on {BindHost}:{BindPort}");
            }
            catch (Exception e)
            {
                OcLogger.Error(e);
            }

        }

        internal void WaitFor()
        {
            _handlerReceive?.TaskReceive?.Wait();
        }
        
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
    }
}