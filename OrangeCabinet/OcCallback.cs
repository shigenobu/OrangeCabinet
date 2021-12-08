namespace OrangeCabinet
{
    public abstract class OcCallback
    {
        public abstract void Incoming(OcRemote remote, byte[] message);
        
        public virtual void Timeout(OcRemote remote) {}
        
        public virtual void Shutdown(OcRemote remote) {}
    }
}