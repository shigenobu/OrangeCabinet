using System;
using System.Collections.Generic;

namespace OrangeCabinet
{
    /// <summary>
    ///     Local for server.
    /// </summary>
    public class OcLocal
    {
        /// <summary>
        ///     Binders.
        ///     Possibly, multi port bind.
        /// </summary>
        private readonly OcBinder[] _binders;

        /// <summary>
        ///     Constructor for array.
        /// </summary>
        /// <param name="binders">binders</param>
        /// <exception cref="OcLocalException">local error</exception>
        public OcLocal(params OcBinder[] binders)
        {
            if (binders.Length > 255)
                throw new OcLocalException("Binders up to 255.");
            _binders = binders;
        }

        /// <summary>
        ///     Constructor for list.
        /// </summary>
        /// <param name="binders">binders</param>
        /// <exception cref="OcLocalException">local error</exception>
        public OcLocal(List<OcBinder> binders) : this(binders.ToArray())
        {
        }

        /// <summary>
        ///     Start.
        /// </summary>
        public void Start()
        {
            foreach (var binder in _binders)
            {
                binder.Bind(OcBindMode.Server);
            }
        }

        /// <summary>
        ///     Wait for.
        /// </summary>
        public void WaitFor()
        {
            foreach (var binder in _binders)
            {
                binder.WaitFor();
            }
        }
        
        /// <summary>
        ///     Shutdown.
        /// </summary>
        public void Shutdown()
        {
            foreach (var binder in _binders)
            {
                binder.Close();
            }
        }
    }
    
    /// <summary>
    ///     Locals exception.
    /// </summary>
    public class OcLocalException : Exception
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="message">message</param>
        internal OcLocalException(string message) : base(message)
        {
        }
    }
}