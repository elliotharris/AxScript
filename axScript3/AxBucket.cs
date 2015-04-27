using System.Collections.Generic;

namespace axScript3
{
    class AxBucket
    {
        private readonly Dictionary<AxVariablePtr, object> _variables = new Dictionary<AxVariablePtr, object>();

        public object Get(AxVariablePtr x)
        {
            return _variables[x];
        }
        public bool TryGet(AxVariablePtr x, out object o)
        {
            return _variables.TryGetValue(x, out o);
        }

        public void Decount()
        {
            
        }
    }
}
