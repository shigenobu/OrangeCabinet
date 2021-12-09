namespace OrangeCabinet
{
    /// <summary>
    ///     Callback.
    /// </summary>
    public abstract class OcCallback
    {
        /// <summary>
        ///     Incoming.
        /// </summary>
        /// <param name="remote">received remote</param>
        /// <param name="message">message</param>
        public abstract void Incoming(OcRemote remote, byte[] message);
        
        /// <summary>
        ///     Timeout.
        /// </summary>
        /// <param name="remote">be timeout remote</param>
        public virtual void Timeout(OcRemote remote) {}
        
        /// <summary>
        ///     Shutdown.
        /// </summary>
        /// <param name="remote">be shutdown remote</param>
        public virtual void Shutdown(OcRemote remote) {}
    }
}