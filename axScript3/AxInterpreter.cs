using System;
using System.Collections.Generic; 
using System.Linq;
using Microsoft.CSharp;
using System.Collections;

namespace axScript3
{
    public class AxInterpreter
    {
        string _scriptStore;
        public Dictionary<String, AxFunction> Functions       = new Dictionary<String, AxFunction>();
        public Dictionary<String, NetFunction> SharpFunctions = new Dictionary<String, NetFunction>();
        public Dictionary<String, object> Variables           = new Dictionary<String, object>();
        public Dictionary<String, Tuple<string, int>> Labels  = new Dictionary<String, Tuple<string, int>>();
        public List<String> CallStack = new List<String>();
        public delegate string SharpFunction(string[] Parameters);
        public string entryPoint;
        
        public AxInterpreter ()
        {
            //Get static functions.
            SharpFunctions = AxSharpFunctions.AddBaseFunctions();
            
            //Instance based functions
            RegisterOwnFunction("set", "Set");
            RegisterOwnFunction("get", "TryGetVar");
            RegisterOwnFunction("addFunc", "AddStaticFunction");
            RegisterOwnFunction("if", "If");
            RegisterOwnFunction("for", "For");
            RegisterOwnFunction("goto", "GotoLabel");
        }
        
        #region Dynamic Functions
        protected void RegisterOwnFunction(string name, string FuncName)
        {
            RegisterFunction(name, new NetFunction(this.GetType().GetMethod(FuncName), this));
        }
        
        public void GotoLabel(string func, string name)
        {
            var axFunc = Functions[func];
            
            foreach(var A in axFunc.Tags)
            {
                if(A.Key == "Label")
                {
                    if(A.Value.Item1 == name)
                    {
                        Console.WriteLine ("goto: {0}:{1}", func, A.Value.Item2);	
                        //(axFunc.InnerFunction.Substring(A.Value.Item2));
                        break;
                    }
                }
            }
        }
        
        public void AddStaticFunction(string AxName, string Func, Type _type)
        {
            RegisterFunction(AxName, new NetFunction(_type.GetMethod(Func)));
        }

        // DEPRECIATED - isset now static and works on function pointers.
        public bool DoesVarExist(String Var)
        {
            return Variables.ContainsKey(Var);
        }
        
        public object TryGetVar(AxVariablePtr Var)
        {
            object output;
            bool success = Variables.TryGetValue(Var, out output);
            if(success) return output;
            else return null;
        }
        
        public void If(bool Cond, AxFunction Then, AxFunction Else)
        {
            if(Cond) 
                Then.Call(this, null); 
            else 
                Else.Call(this, null);
        }
        
        public void While(AxFunction Cond, AxFunction Do)
        {
            while(((bool)Cond.Call(this, null) == true)) Do.Call(this, null);	
        }
        
        public void For(object range, AxFunction func)
        {
            var a = (IEnumerable<object>)range;
            foreach(var v in a)
            {
                var b = new Dictionary<string, object> { {func.Parameters[0], v } };
                func.Call(this, b);	
            }
        }
                
        public void RegisterFunction(String name, NetFunction func)
        {
            SharpFunctions.Add(name, func);	
        }
        
        public void Set(AxVariablePtr Var, params object[] Values)
        {
            if (Var.Type == VariableType.Local) throw Error("Cannot use global set to set local variable \"{0}\", please use local set instead.", Var);
            object _Values;
            if(Values.Length == 1)
            {
                _Values = Values[0];
            }
            else
            {
                _Values = new List<object>(Values);
            }
            if(Variables.ContainsKey(Var)) Variables[Var] = _Values;
            else Variables.Add(Var, _Values);
        }
        #endregion
        
        #region Interpreter Functions
        public void Run(string Script)
        {
            Console.WriteLine ("axScriptX Interpreter r355 (c) Blam 2011-2012");
            Console.WriteLine ("#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~#");
            
            //Backup for later.
            _scriptStore = Script;
            Script = StripComments(Script);
            
            getFunctions(Script);
            //try
            //{
                callFunction(entryPoint, new List<object> { "axScriptX 3" });
            //}
            //catch (Exception ex)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine("\n\n\nERROR RUNNING SCRIPT:\n\t{0}", ex.Message);
            //    Console.Write("Call History:\n\t");
            //    CallStack.ForEach(x => Console.Write("{0}, ", x));
            //    Console.WriteLine("[error]");
            //    Console.ReadLine();
            //    throw;
            //}
        }
        
        public object callFunction(String func, List<object> parameters)
        {
            CallStack.Add(func);
            if(Functions.ContainsKey(func))
            {
                var AxFunc = Functions[func];
                int paramC = AxFunc.ParamCount;
                Dictionary<String, object> Params = new Dictionary<String, object>();
                
                if(!AxFunc.FixedParams)
                {
                    List<object> args = new List<object>();
                    for(int i = paramC-1; i < parameters.Count; i++) args.Add(parameters[i]);
                    Params.Add("args", args);
                }
                
                Params.Add("return", null); // default return;
                for(int i = 0; i < paramC; i++) if(AxFunc.Parameters[i] != "...") Params.Add(AxFunc.Parameters[i], parameters[i]);
                var ret = Functions[func].Call(this, Params);	
                return ret;
            }
            else if(SharpFunctions.ContainsKey(func))
            {
                var ret = SharpFunctions[func].call(parameters.ToArray());
                return ret;
            }
            
            throw Error("Function \"{0}\" not found!", func);
        }

        public AxException Error(String message, params object[] p)
        {
            if (p != null) message = String.Format(message, p);
            return new AxException(this, message);
        }
        
        public object callFuncFromString(String funcString, Dictionary<String, object> Params)
        {
            int spaceIndex = funcString.IndexOf(' ');
            int oSpaceIndex = spaceIndex;
            List<object> parameters = new List<object>();
            String funcName;
            if(spaceIndex != -1)
            {
                funcName = funcString.Substring(0, spaceIndex);				
                
                while(spaceIndex < funcString.Length)
                {
                    int start = oSpaceIndex+1;
                    int end = spaceIndex;
                    
                    while(funcString[start] == ' ') start++;
                    
                    if(funcString[start] == '"') // String
                    {
                        int off = start;
                        do { end = funcString.IndexOf("\"", off+1); off=end; }
                            while(funcString[end-1] == '\\');

                        parameters.Add(funcString.Substring(start+1, end-start-1).Replace("\\\"", "\"").Replace("\\n", "\n"));	
                        end++;
                        if(end > funcString.Length) end = funcString.Length;
                    }
                    else if(char.IsDigit(funcString[start]) || funcString[start] == '-' && char.IsDigit(funcString[start+1])) // Number
                    {
                        end = funcString.IndexOf(" ", start);
                        if(end == -1) end = funcString.Length;
                        double paramDbl;
                        string num = funcString.Substring(start, end-start);
                        if(double.TryParse(num, out paramDbl)) parameters.Add(paramDbl);
                        else throw Error("Malformed number found.");
                    }
                    else if(funcString[start] == '(') // Function call
                    {
                        var param = extract(funcString.Substring(start));
                        parameters.Add(callFuncFromString(param.Item1, Params));
                        end = start + param.Item2;
                    }
                    else if(funcString[start] == '{') // Lambda
                    {
                        var extr = extract(funcString.Substring(start), '{', '}');
                        string func = extr.Item1;
                        string[] _parameters = new string[0];
                        int paramindex = func.IndexOf('|');
                        if(paramindex != -1)
                        {
                            _parameters = func.Substring(0, paramindex).Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                            func = func.Substring(paramindex+1);
                        }
                        
                        parameters.Add(new AxFunction(_parameters, func, true));
                        end = start + extr.Item2;
                    }
                    else if(funcString[start] == '[') // Range
                    {
                        var extr = extract(funcString.Substring(start), '[', ']');
                        var str = extr.Item1;
                        
                        if(str.IndexOf(',') != -1)
                        {
                            IEnumerable<double> a = str.Split(',').Select(x => double.Parse(x));
                            parameters.Add(a);
                        }
                        else if(str.IndexOf("..") != -1)
                        {
                            int rangediff = 1;
                            string[] temp;
                            if(str.IndexOf('|') != -1)
                            {
                                temp = str.Split('|');
                                rangediff = int.Parse(temp[1]);
                                str = temp[0];
                            }
                            
                            temp = str.Split(new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries);
                            int rangestart = int.Parse(temp[0]);
                            int rangeend = int.Parse(temp[1]);
                            
                            IEnumerable<int> dbl = CreateRange(rangestart, rangeend, rangediff);
                            
                            parameters.Add(dbl);
                        }
                        
                        end = start + extr.Item2;
                    }
                    else if (funcString[start] == '*') //function pointer
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1) end = funcString.Length;

                        string ptrfunc = funcString.Substring(start+1, end - start -1);

                        if (Functions.ContainsKey(ptrfunc)) parameters.Add(Functions[ptrfunc]);
                        else throw Error("Invalid function refference: \"{0}\", function not found.", ptrfunc);
                    }
                    else if (funcString[start] == '^') //variable pointer
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1) end = funcString.Length;

                        string Variable = funcString.Substring(start + 1, end - start - 1);

                        VariableType varType = VariableType.Null;
                        if (Params.ContainsKey(Variable))
                        {
                            varType = VariableType.Local;
                        }
                        else if (Variables.ContainsKey(Variable))
                        {
                            varType = VariableType.Global;
                        }

                        AxVariablePtr varptr = new AxVariablePtr(Variable, varType);

                        parameters.Add(varptr);
                    }
                    else if (funcString[start] == 't') //true keyword
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1) end = funcString.Length;

                        string _par = funcString.Substring(start, end - start);

                        if (_par == "true")
                            parameters.Add(true);
                    }
                    else if (funcString[start] == 'f') // false keyword
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1) end = funcString.Length;
                        if (funcString.Substring(start, end - start) == "false")
                            parameters.Add(false);
                    }
                    else if (funcString[start] == '@') // iterator
                    {
                        start++;
                        object Var;
                        string varName;
                        end = funcString.IndexOf(" ", start);
                        if (end == -1) end = funcString.Length;
                        var locindex = funcString.IndexOf("[", start, end - start);
                        var loc2index = funcString.IndexOf(".", start + 1, end - start - 1);
                        if (loc2index != -1 && loc2index < locindex) locindex = loc2index;
                        if (locindex != -1) varName = funcString.Substring(start, locindex - start);
                        else varName = funcString.Substring(start, end - start);

                        Var = GetVar(varName, Params);
                        ExtrapolateVariable(funcString, end, ref Var, ref locindex);

                        parameters.Add((IEnumerable<object>)Var);
                    }
                    else if (funcString[start] == '$') //get global variable always, even if local exists with same name.
                    {
                        object Var;
                        string varName;
                        end = funcString.IndexOf(" ", start);
                        if (end == -1) end = funcString.Length;
                        var locindex = funcString.IndexOf("[", start + 1, end - start - 1);
                        var loc2index = funcString.IndexOf(".", start + 1, end - start - 1);
                        if (loc2index != -1 && loc2index < locindex) locindex = loc2index;
                        if (locindex != -1) varName = funcString.Substring(start + 1, locindex - start - 1);
                        else varName = funcString.Substring(start + 1, end - start - 1);

                        Var = Variables[varName];
                        ExtrapolateVariable(funcString, end, ref Var, ref locindex);

                        parameters.Add(Var);
                    }
                    else //local has priority.
                    {
                        object Var;
                        string varName;
                        end = funcString.IndexOf(" ", start);
                        if (end == -1) end = funcString.Length;
                        var locindex = funcString.IndexOf("[", start, end - start);
                        var loc2index = funcString.IndexOf(".", start + 1, end - start - 1);
                        
                        if (loc2index != -1 && (loc2index < locindex || locindex == -1)) locindex = loc2index;
                        if (locindex != -1) varName = funcString.Substring(start, locindex - start);
                        else varName = funcString.Substring(start, end - start);

                        Var = GetVar(varName, Params);
                        ExtrapolateVariable(funcString, end, ref Var, ref locindex);

                        parameters.Add(Var);
                    }
                    
                    
                    oSpaceIndex = end;
                    
                    spaceIndex = end + 1;
                }
            }
            else funcName = funcString; 

            // HACK: Remove hacky lset.
            if(funcName == "lset") 
            {
                string Var = ((AxVariablePtr)parameters[0]).ToString();
                object Value = parameters[1];
                if(Params.ContainsKey(Var)) Params[Var] = Value;
                else Params.Add(Var, Value);
                
                return null;
            }
            else return callFunction(funcName, parameters);
        }

        // Checks for element indexers, and loops through them all to get to the data desired.
        private void ExtrapolateVariable(String funcString, int end, ref object Var, ref int locindex)
        {
            if (locindex == -1) return;

            while (locindex < end)
            {
                char c = funcString[locindex];
                if (c == '[')
                {
                    int locIndexEnd = funcString.IndexOf("]", locindex + 1, end - locindex - 1);
                    string indexerStr = funcString.Substring(locindex + 1, locIndexEnd - locindex - 1);
                    int indexer = int.Parse(indexerStr);
                    var Enum = ((IEnumerable)Var).GetEnumerator();
                    Enum.MoveNext();

                    for (int i = 0; i < indexer; i++)
                    {
                        if(!Enum.MoveNext()) throw Error("Invalid indexer specified, it is larger than the collection.");
                    }

                    Var = Enum.Current;

                    Enum.Reset();

                    locindex = locIndexEnd + 1;
                }
                else if (c == '.')
                {
                    int locIndexEnd = funcString.IndexOf("[", locindex + 1, end - locindex - 1);
                    int locIndexEnd2 = funcString.IndexOf(".", locindex + 1, end - locindex - 1);
                    if (locIndexEnd2 != -1 && locIndexEnd2 < locIndexEnd) locIndexEnd = locIndexEnd2;
                    if (locIndexEnd == -1) locIndexEnd = end; //hit the end
                    string indexerStr = funcString.Substring(locindex + 1, locIndexEnd - locindex - 1);
                    Type t = Var.GetType();

                    dynamic prop = t.GetField(indexerStr);
                    if (prop == null) prop = t.GetProperty(indexerStr);

                    Var = prop.GetValue(Var);
                    locindex = locIndexEnd;
                }
                else
                {
                    break;
                }
            }
        }
        
        public static IEnumerable<int> CreateRange(int start, int end, int difference=1)
        {
            List<int> nums = new List<int>();
            while(start <= end)
            {
                nums.Add(start);
                start += difference;	
            }
            
            return nums;
        }

        protected object GetVar(string Variable, Dictionary<string, object> Local)
        {
            VariableType varType = VariableType.Null;
            if (Local.ContainsKey(Variable))
            {
                varType = VariableType.Local;
            }
            else if (Variables.ContainsKey(Variable))
            {
                varType = VariableType.Global;
            }
            dynamic Var;
            switch (varType)
            {
                case VariableType.Global:
                    Var = Variables[Variable];
                    break;
                case VariableType.Local:
                    Var = Local[Variable];
                    break;
                default:
                    throw Error(String.Format("Variable \"{0}\" not found.", Variable));
            }

            return Var;
        }
        
        public enum VariableType
        {
            Null = -1,
            Local,
            Global,
        }
        
        protected void getFunctions(string Script)
        {
            for(int i = 0; i < Script.Length; i++)
            {
                if(Script[i] == '~' && Script[i+1] == '(')
                {
                    var func = extract(Script.Substring(i));
                    if(func.Item2 != -1)
                    {
                        
                        string FunctionString = func.Item1;
                        
                        int indexOfColon = FunctionString.IndexOf(':');
                        int indexOfParenthesis = FunctionString.IndexOf('(');
                        int indexOfPeriod = FunctionString.IndexOf('|');
                        
                        string FunctionName = FunctionString.Substring(0, indexOfColon);
                        //Console.WriteLine (FunctionName);
                        string[] FunctionParameters;
                        string FunctionContents;
                        if(indexOfPeriod != -1 && indexOfParenthesis > indexOfPeriod)
                        {
                            FunctionParameters = FunctionString.Substring(indexOfColon+1, indexOfPeriod-indexOfColon-1).Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                            FunctionContents = FunctionString.Substring(indexOfPeriod+1);
                        }
                        else
                        {
                            FunctionParameters = new string[0];
                            FunctionContents = FunctionString.Substring(indexOfParenthesis);
                        }
                        bool FunctionFixedParams = !FunctionParameters.Contains("...");
                        var AxFunc = new AxFunction(FunctionParameters, FunctionContents, FunctionFixedParams);
                        AxFunc.GetDynamicTags();
                        Functions.Add(FunctionName, AxFunc);
                        if(AxFunc.Tags.ContainsKey("EntryPoint")) entryPoint = FunctionName;
                        i+= func.Item2;
                    }
                    else
                    {
                        Console.WriteLine ("Invalid function");	
                    }
                }
            }
        }
        #endregion

        #region Helper Functions
        public static Tuple<string, int> extract(string Script)
        {
            return extract(Script, '(', ')');
        }
        
        public static Tuple<string, int> extract(string Script, char startblock, char endblock, bool escapeCounts=false)
        {
            //Add one to par each time we hit a startblock
            //Take one from par each time we hit an endblock
            //Therefore when it gets to 0 we reach the endblock our startblock relates to.
            int par = 1; 
            int i = 0; //substring start
            int j = 0; //substring end
            
            while(i < Script.Length && Script[i] != startblock) i++;
            
            if(i != Script.Length)
            {
                while(par > 0)
                {
                    j++;
                    if(Script[j+i] == endblock) par--;
                    else if(Script[j+i] == startblock) par++;
                }
                return new Tuple<string, int>(Script.Substring(i+1,j-1), j );
            }
            else
            {
                return new Tuple<string, int>("", -1 );	
            }
        }
        
        public string StripComments(string Script)
        {
            int j = 0;
            for(int i = Script.Length-1; i > 0; i--)
            {				
                //Remove Comments
                if(Script[i] == '/')
                    if(Script[i-1] == '/')
                {
                    j = 1;
                    while (j + i < Script.Length && !isNewLine(Script[j + i]))
                    {
                        j++;
                    }
                    Script = Script.Remove(i-1,j+2);
                }
            }
            
            //Remove Newlines and unimportant whitespace
            for(int i = Script.Length-1; i > -1; i--)
                if(isNewLine(Script[i]) || Script[i] == '\t' || ( i > 0 && Script[i] == ' ' && Script[i-1] == ' ' )) Script = Script.Remove(i, 1);
            
            return Script;
        }

        public static bool isNewLine(char c)
        {
            if (c == '\n' || c == '\r') return true;

            return false;
        }
        #endregion

        #region Overrides
        public object this[string varName]
        {
            get { return Variables[varName]; }
            set { Variables[varName] = value; }

        }
        #endregion
    }
}

