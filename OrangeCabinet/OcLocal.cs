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
        ///     binder.
        /// </summary>
        private readonly OcBinder _binder;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="binder">binder</param>
        public OcLocal(OcBinder binder)
        {
            _binder = binder;
        }

        /// <summary>
        ///     Start.
        /// </summary>
        public void Start()
        {
            _binder.Bind(OcBindMode.Server);
        }

        /// <summary>
        ///     Wait for.
        /// </summary>
        public void WaitFor()
        {
            _binder.WaitFor();
        }
        
        /// <summary>
        ///     Shutdown.
        /// </summary>
        public void Shutdown()
        {
            _binder.Close();
        }
    }
}