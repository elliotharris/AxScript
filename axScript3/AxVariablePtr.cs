namespace axScript3
{
    public class AxVariablePtr
    {
        public string VariableName;
        public AxInterpreter.VariableType Type;

        public AxVariablePtr(string variable, AxInterpreter.VariableType varType)
        {
            VariableName = variable;
            Type = varType;
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
