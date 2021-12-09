using System.Collections.Generic;

namespace OrangeCabinet
{
    public class OcLocal
    {
        private readonly OcBinder[] _binders;

        public OcLocal(params OcBinder[] binders)
        {
            _binders = binders;
        }

        public OcLocal(List<OcBinder> binders) : this(binders.ToArray())
        {
        }

        public void Start()
        {
            foreach (var binder in _binders)
            {
                binder.Bind();
            }
        }

        public void WaitFor()
        {
            foreach (var binder in _binders)
            {
                binder.WaitFor();
            }
        }
        
        public void Shutdown()
        {
            foreach (var binder in _binders)
            {
                binder.Close();
            }
        }
    }
}