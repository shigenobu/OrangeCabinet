namespace OrangeCabinet
{
    public class OcLocal
    {
        private OcBinder _binder;

        public OcLocal(OcBinder binder)
        {
            _binder = binder;
        }

        public void Start()
        {
            _binder.Bind();
        }

        public void WaitFor()
        {
            _binder.WaitFor();
        }
        
        public void Shutdown()
        {
            _binder.Close();
        }
    }
}