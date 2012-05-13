using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace axScript3
{
    public class AxVariablePtr
    {
        public string VariableName;
        public AxInterpreter.VariableType Type;

        public AxVariablePtr(string Variable, AxInterpreter.VariableType varType)
        {
            this.VariableName = Variable;
            this.Type = varType;
        }

        public override string ToString()
        {
            return (string)this;
        }

        public static implicit operator string(AxVariablePtr ptr)
        {
            return ptr.VariableName;
        }
    }
}
