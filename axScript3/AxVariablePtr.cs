namespace axScript3
{
    public class AxVariablePtr
    {
        public readonly AxInterpreter.VariableType Type;
        public readonly string VariableName;
        public int Count;

        public AxVariablePtr(string variable, AxInterpreter.VariableType varType)
        {
            VariableName = variable;
            Type = varType;
        }

        public override string ToString()
        {
            return VariableName;
        }

        public static implicit operator string(AxVariablePtr ptr)
        {
            return ptr.VariableName;
        }
    }
}