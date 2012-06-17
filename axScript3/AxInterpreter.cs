using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.CSharp;
using System.Collections;
using System.Text;

namespace axScript3
{
    public class AxInterpreter
    {
        public String Script;
        public Dictionary<String, AxFunction> Functions       = new Dictionary<String, AxFunction>();
        public Dictionary<String, NetFunction> SharpFunctions = new Dictionary<String, NetFunction>();
        public Dictionary<String, object> Variables           = new Dictionary<String, object>();
        public Dictionary<String, Tuple<string, int>> Labels  = new Dictionary<String, Tuple<string, int>>();
        public List<String> CallStack = new List<String>();
        public delegate string SharpFunction(string[] parameters);
        public string EntryPoint;
        public bool Debug;
        private string _scriptStore;
        public List<String> Modules = new List<string>();

        public AxInterpreter (Boolean debug = false)
        {
            Debug = debug;

            //Get static functions.
            SharpFunctions = AxSharpFunctions.AddBaseFunctions();
            
            //Instance based functions
            RegisterOwnFunction("set", "Set");
            RegisterOwnFunction("get", "TryGetVar");
            RegisterOwnFunction("addFunc", "AddStaticFunction");
            RegisterOwnFunction("if", "If");
            RegisterOwnFunction("while", "While");
            RegisterOwnFunction("do", "Do");
            RegisterOwnFunction("ifelse", "IfElse");
            RegisterOwnFunction("for", "For");
            RegisterOwnFunction("goto", "GotoLabel");
            RegisterOwnFunction("array", "Array");
        }
        
        #region Dynamic Functions
        protected void RegisterOwnFunction(string name, string FuncName)
        {
            RegisterFunction(name, new NetFunction(this.GetType().GetMethod(FuncName), this));
        }

        //public void AddFuncToEvent(object eventClass, string eventName, AxFunction f)
        //{
        //    Type classType = eventClass.GetType();
        //    var eventInfo = classType.GetEvent(eventName);
        //    var paramsInfo = eventInfo.GetRaiseMethod().GetParameters();
        //    var paramTypes = paramsInfo.Select(x => x.GetType()).ToArray();
        //    var methodToCall = new MethodBuilder("runScriptedFunction", MethodAttributes.Public, CallingConventions.Any, null, paramTypes, this.GetType());
        //    //
        //    // Code to build method - this is where I'm unsure of what to do
        //    //
        //    eventInfo.AddEventHandler(eventClass, methodToCall);
        //}
        
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
            return null;
        }
        
        public void If(bool cond, AxFunction then)
        {
            if (cond) then.Call(this, null);
        }

        public void IfElse(bool cond, AxFunction then, AxFunction elseF)
        {
            if(cond) 
                then.Call(this, null); 
            else 
                elseF.Call(this, null);
        }
        
        public void While(AxFunction cond, AxFunction Do)
        {
            while((bool)cond.Call(this, null)) Do.Call(this, null);	
        }

        public void Do(AxFunction Do, AxFunction cond)
        {
            do { Do.Call(this, null); } while ((bool)cond.Call(this, null));
        }
        
        public void For(object range, AxFunction func)
        {
            var r = (IEnumerable)range;
            foreach(var v in r)
            {
                Dictionary<string, object> b = func.ParamCount == 1 ? new Dictionary<string, object> {{func.Parameters[0], v}} : new Dictionary<string, object>();
                func.Call(this, b);
            }
        }
                
        public void RegisterFunction(String name, NetFunction func)
        {
            SharpFunctions.Add(name, func);	
        }
        
        public object Set(AxVariablePtr var, params object[] values)
        {
            if (var.Type == VariableType.Local) throw Error("Cannot use global set to set local variable \"{0}\", please use local set instead.", var);
            object nValues;
            if(values.Length == 1)
            {
                nValues = values[0];
            }
            else
            {
                nValues = new List<object>(values);
            }
            if(Variables.ContainsKey(var)) Variables[var] = nValues;
            else Variables.Add(var, nValues);

            return Variables[var];
        }

        public object Array(params object[] values)
        {
            return new List<object>(values);
        }

        #endregion
        
        #region Interpreter Functions
        public void Run(string scriptText)
        {
            this.Script = scriptText;
            //Backup for later.
            _scriptStore = scriptText;
            FirstPass();
            getFunctions();
            if(EntryPoint == null) throw new AxException(this, "No entry point found");
            CallFunction(EntryPoint, new List<object> { "axScriptX 3" }, new Dictionary<string, object>());
        }
        
        public object CallFunction(String func, List<object> parameters, Dictionary<string, object> locals)
        {
            CallStack.Add(func);

            //Class Function Call
            int colonPos = func.IndexOf(':');
            if (colonPos != -1)
            {
                var locindex = func.IndexOf("[", 0, colonPos);
                var loc2index = func.IndexOf(".", 0, colonPos);
                string varName = "";
                if (loc2index != -1 && (locindex == -1 || loc2index < locindex)) locindex = loc2index;
                if (locindex != -1) varName = func.Substring(0, locindex);
                else varName = func.Substring(0, colonPos);
                object Var = GetVar(varName, locals);
                int spacePos = func.IndexOf(' ', colonPos);
                if (spacePos == -1) spacePos = func.Length - 1;
                spacePos -= colonPos;
                string Method = func.Substring(colonPos+1, spacePos);
                ExtrapolateVariable(func, colonPos, ref Var, ref locindex);
                Type[] paramTypes = parameters.Select(p => p.GetType()).ToArray();
                return new NetFunction(Var.GetType().GetMethod(Method, paramTypes), Var).call(parameters.ToArray());
            }

            if(Functions.ContainsKey(func))
            {
                var AxFunc = Functions[func];
                int paramC = AxFunc.ParamCount;
                var Params = new Dictionary<String, object>();
                
                if(!AxFunc.FixedParams)
                {
                    var args = new List<object>();
                    for(int i = paramC-1; i < parameters.Count; i++) args.Add(parameters[i]);
                    Params.Add("params", args);
                }
                
                Params.Add("return", null); // default return;
                try
                {
                    for(var i = 0; i < paramC; i++) if(AxFunc.Parameters[i] != "...") Params.Add(AxFunc.Parameters[i], parameters[i]);
                }
                catch (Exception)
                {
                    throw new AxException(this, "Parameter count missmatch while calling: \""+func+"\\");
                }
                var ret = Functions[func].Call(this, Params);	
                return ret;
            }

            if(SharpFunctions.ContainsKey(func))
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
        
        public object CallFuncFromString(String funcString, Dictionary<String, object> Params)
        {
            int spaceIndex = funcString.IndexOf(' ');
            int oSpaceIndex = spaceIndex;
            var parameters = new List<object>();
            String funcName;
            if(spaceIndex != -1)
            {
                funcName = funcString.Substring(0, spaceIndex);				
                
                while(spaceIndex < funcString.Length)
                {
                    int end;
                    int start;
                    oSpaceIndex = ParseParameter(funcString, Params, parameters, oSpaceIndex, out end, ref spaceIndex, out start);
                }
            }
            else funcName = funcString;

            if (funcName == "")
                return null;

            // HACK: Remove hacky lset.
            if(funcName == "lset") 
            {
                string var = parameters[0].ToString();
                object value = parameters[1];
                if(Params.ContainsKey(var)) Params[var] = value;
                else Params.Add(var, value);
                
                return null;
            }
            if (funcName == "return")
            {
                object value = parameters[0];
                if (Params.ContainsKey("return")) Params["return"] = value;
                else Params.Add("return", value);

                return value;
            }
            return CallFunction(funcName, parameters, Params);
        }

        private int ParseParameter(string funcString, Dictionary<string, object> Params, List<object> parameters, int oSpaceIndex, out int end, ref int spaceIndex, out int start)
        {
            start = oSpaceIndex + 1;
            end = spaceIndex;

            while (funcString[start] == ' ' || funcString[start] == '\n') start++;

            if (funcString[start] == '"') // String
            {
                var sb = new StringBuilder();
                int j = start + 1;
                bool escapeState = false;
                while (funcString[j] != '"' || escapeState)
                {
                    if (escapeState)
                    {
                        if (funcString[j] == 'n') sb.Append('\n');
                        else if (funcString[j] == 'r') sb.Append('\r');
                        else if (funcString[j] == '"') sb.Append('"');
                        else if (funcString[j] == 't') sb.Append('\t');
                        else if (funcString[j] == '0') sb.Append('\0');
                        else if (funcString[j] == '\\') sb.Append('\\');
                        else throw new AxException(this, "Malformed escape sequence");
                        escapeState = false;
                        j++;
                        continue;
                    }
                    if (funcString[j] == '\\')
                    {
                        escapeState = true;
                    }
                    else
                    {
                        sb.Append(funcString[j]);
                    }
                    j++;
                }

                parameters.Add(sb.ToString());
                end = j + 1;
                if (end > funcString.Length) end = funcString.Length;
            }
            else if (char.IsDigit(funcString[start]) || funcString[start] == '-' && char.IsDigit(funcString[start + 1])) // Number
            {
                end = funcString.IndexOf(" ", start);
                if (end == -1) end = funcString.Length;
                string num = funcString.Substring(start, end - start);
                if (num.StartsWith("0x"))
                {
                    int paramInt;
                    if (int.TryParse(num.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out paramInt)) parameters.Add(paramInt);
                    else throw Error("Malformed hex '{0}' found.", num);
                }
                else if (num.EndsWith("l"))
                {
                    long paramLong;
                    if (long.TryParse(num.Substring(0, num.Length - 1), out paramLong)) parameters.Add(paramLong);
                    else throw Error("Malformed long '{0}' found.", num);
                }
                else if (num.EndsWith("f"))
                {
                    float paramFloat;
                    if (float.TryParse(num.Substring(0, num.Length - 1), out paramFloat)) parameters.Add(paramFloat);
                    else throw Error("Malformed float '{0}' found.", num);
                }
                else
                {
                    int paramInt;
                    if (num.Contains('.'))
                    {
                        double paramDbl;
                        if (double.TryParse(num, out paramDbl)) parameters.Add(paramDbl);
                        else throw Error("Malformed double '{0}' found.", num);
                    }
                    else
                    {
                        if (int.TryParse(num, out paramInt)) parameters.Add(paramInt);
                        else throw Error("Malformed int '{0}' found.", num);
                    }
                }
            }
            else if (funcString[start] == '(') // Function call
            {
                var param = extract(funcString.Substring(start));
                parameters.Add(CallFuncFromString(param.Item1, Params));
                end = start + param.Item2;
            }
            else if (funcString[start] == '{') // Lambda
            {
                var extr = extract(funcString.Substring(start), '{', '}');
                string func = extr.Item1;
                string[] _parameters = new string[0];
                int paramindex = 0;
                while (paramindex < func.Length && func[paramindex] != '|')
                {
                    if (func[paramindex] == '(')
                    {
                        paramindex = -1;
                        break;
                    }
                    paramindex++;
                }
                if (paramindex != -1)
                {
                    _parameters = func.Substring(0, paramindex).Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    func = func.Substring(paramindex + 1);
                }

                parameters.Add(new AxFunction(_parameters, func, true));
                end = start + extr.Item2;
            }
            else if (funcString[start] == '%') // Return Lambda
            {
                var extr = extract(funcString.Substring(start + 1), '{', '}');
                string func = extr.Item1;
                string[] _parameters = new string[0];
                int paramindex = 0;
                while (func[paramindex] != '|')
                {
                    paramindex++;
                    if (paramindex >= func.Length || func[paramindex] == '(')
                    {
                        paramindex = -1;
                        break;
                    }
                }
                if (paramindex != -1)
                {
                    _parameters = func.Substring(0, paramindex).Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    func = func.Substring(paramindex + 1);
                }

                parameters.Add(new AxFunction(_parameters, String.Format("(return ({0}))", func), true));
                end = start + extr.Item2 + 1;
            }
            else if (funcString[start] == '[') // Range
            {
                var extr = extract(funcString.Substring(start), '[', ']');
                var str = extr.Item1;

                if (str.IndexOf(',') != -1)
                {
                    IEnumerable<double> a = str.Split(',').Select(x => double.Parse(x));
                    parameters.Add(a);
                }
                else if (str.IndexOf("..") != -1)
                {
                    int rangediff = 1;
                    string[] temp;
                    if (str.IndexOf('|') != -1)
                    {
                        temp = str.Split('|');
                        rangediff = int.Parse(temp[1]);
                        str = temp[0];
                    }

                    temp = str.Split(new string[] {".."}, StringSplitOptions.RemoveEmptyEntries);
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

                string ptrfunc = funcString.Substring(start + 1, end - start - 1);

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
            else if (start + 4 <= funcString.Length && funcString.Substring(start, 4) == "true") //true keyword
            {
                end = funcString.IndexOf(" ", start);
                if (end == -1) end = funcString.Length;

                string _par = funcString.Substring(start, end - start);

                if (_par == "true")
                    parameters.Add(true);
            }
            else if (start + 5 <= funcString.Length && funcString.Substring(start, 5) == "false") // false keyword
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

                parameters.Add((IEnumerable<object>) Var);
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
            return oSpaceIndex;
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
        
        public IEnumerable<int> CreateRange(int start, int end, int difference=1)
        {
            var nums = new List<int>();
            if (difference == 1)
            {
                if (start > end)
                {
                    difference = -1;
                }
            }

            if (start > end)
            {
                if (difference >= 0) throw Error("Infinite range found: [{0}..{1}|{2}]", start, end, difference);
                while (start >= end)
                {
                    nums.Add(start);
                    start += difference;
                }
            }
            else
            {
                if (difference <= 0) throw Error("Infinite range found: [{0}..{1}|{2}]", start, end, difference);
                while (start <= end)
                {
                    nums.Add(start);
                    start += difference;
                }
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
                    throw new AxException(this, String.Format("Variable \"{0}\" not found.", Variable));
            }

            return Var;
        }
        
        public enum VariableType
        {
            Null = -1,
            Local,
            Global,
        }
        
        protected void getFunctions()
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
                        if(AxFunc.Tags.ContainsKey("EntryPoint")) EntryPoint = FunctionName;
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

        public void FirstPass()
        {
            StringBuilder sb = new StringBuilder();
            bool inStr = false;
            bool inComment = false;
            bool inCompStatement = false;
            StringBuilder compStateBuffer = new StringBuilder();
            for (int i = 0; i < Script.Length-1; i++)
            {
                if (Script[i] == '\n')
                {
                    inComment = false;
                    if (inCompStatement)
                    {
                        inCompStatement = false;
                        var str = compStateBuffer.ToString();
                        int split = str.IndexOf(' ');
                        if (split != -1)
                        {
                            ParseCompilerStatement(str.Substring(0, split), str.Substring(split + 1));
                        }
                        else
                        {
                            ParseCompilerStatement(str, null);
                        }
                        compStateBuffer.Clear();
                    }
                }
                //Toggle String Mode
                if (!inComment)
                {
                    if (Script[i+1] == '"')
                        if (Script[i] != '/')
                        {
                            inStr = inStr ? false : true;
                        }
                    if (!inStr)
                    {
                        //Remove Comments
                        if (Script[i] == '/')
                            if (Script[i + 1] == '/')
                            {
                                inComment = true;
                                continue;
                            }
                        //Remove and store Compiler Statements
                        if (Script[i] == '#')
                        {
                            inCompStatement = true;
                            continue;
                        }
                    }

                    if (inCompStatement)
                    {
                        compStateBuffer.Append(Script[i]);
                        continue;
                    }

                    if (!(isNewLine(Script[i]) || Script[i] == '\t' || (i > 0 && Script[i] == ' ' && Script[i - 1] == ' ')))
						sb.Append(Script[i]);
                }

                
            }

            Script = sb.ToString();
        }

        private void ParseCompilerStatement(string statement, string p)
        {
            statement = statement.ToLower();
            switch (statement)
            {
                case "module":
                    AxModuleLoader.Load(this, p);
                    break;
                default:
                    break;
            }
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

        /// <summary>
        /// Clears all of the variables and AxNet functions, keeps .NET functions and modules.
        /// </summary>
        public void Clear()
        {
            Functions.Clear();
            Variables.Clear();
            CallStack.Clear();
        }
    }
}

