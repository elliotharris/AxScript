namespace axScript3
{
    public class AxVariablePtr
    {
        public AxInterpreter.VariableType Type;
        public string VariableName;

        public AxVariablePtr(string variable, AxInterpreter.VariableType varType)
        {
            VariableName = variable;
            Type = varType;
        }

        public override string ToString()
        {
            return this;
        }

        public static implicit operator string(AxVariablePtr ptr)
        {
            return ptr.VariableName;
        }
    }
}