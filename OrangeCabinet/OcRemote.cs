using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace OrangeCabinet
{
    public class OcRemote
    {
        
        
        private OcBinder _binder;

        public IPEndPoint RemoteEndpoint { get; }

        private readonly string _rid;

        /// <summary>
        ///     Idle milli seconds.
        /// </summary>
        private int _idleMilliSeconds = 10000;
        
        /// <summary>
        ///     Life timestamp milli seconds.
        /// </summary>
        private long _lifeTimestampMilliseconds;

        internal bool Active { get; set; } = true;
 

        /// <summary>
        ///     Newest.
        /// </summary>
        private bool _newest = true;

        /// <summary>
        ///     Session values.
        /// </summary>
        private Dictionary<string, object>? _values;
        
        public OcRemote(OcBinder binder, string remoteHost, int remotePort)
            : this(binder, new IPEndPoint(IPAddress.Parse(remoteHost), remotePort))
        {
        }

        public OcRemote(OcBinder binder, IPEndPoint remoteEndpoint)
        {
            _binder = binder;
            RemoteEndpoint = remoteEndpoint;

            _rid = OcUtils.RandomString(16);
            _lifeTimestampMilliseconds = OcDate.NowTimestampMilliSeconds() + _idleMilliSeconds;
        }
        
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

        public void Send(byte[] message)
        {
            // bin
            _binder.Bind();
            
            // if escaped, disallow send
            if (_lifeTimestampMilliseconds <= 0) {
                throw new OcSendException($"Remote({this}) is already escaped");
            }

            // if not active, disallow send
            if (!Active) {
                throw new OcSendException($"Remote({this}) is not active");
            }

            try
            {
                _binder.BindSocket!.SendTo(message, SocketFlags.None, RemoteEndpoint);
            } catch (Exception e) {
                OcLogger.Error(e);
                throw new OcSendException(e);
            }
        }
        
        public void Escape() {
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
            return (T?)_values[name];
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
            return $"Rid:{_rid}, HostPort:{RemoteEndpoint.OxToHostPort()}";
        }
    }
    
    /// <summary>
    ///     Send exception.
    /// </summary>
    public class OcSendException : Exception
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="e">exception</param>
        internal OcSendException(Exception e) : base(e.ToString())
        {}
        
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="message">message</param>
        internal OcSendException(string message) : base(message)
        {}
    }
}