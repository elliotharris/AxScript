using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace axScript3
{
    public class AxInterpreter
    {
        #region Delegates
        public delegate bool ScriptEndEvent(Exception e);
        #endregion

        public readonly List<String> CallStack = new List<String>();

        public readonly bool Debug;
        public string EntryPoint;
        public readonly Dictionary<String, AxFunction> Functions = new Dictionary<String, AxFunction>();
        public readonly Dictionary<String, NetFunction> Hooks = new Dictionary<String, NetFunction>();
        public Dictionary<String, Tuple<string, int>> Labels = new Dictionary<String, Tuple<string, int>>();
        public readonly List<String> Modules = new List<string>();
        public String Script;
        public readonly Dictionary<String, NetFunction> SharpFunctions = new Dictionary<String, NetFunction>();
        public readonly Dictionary<String, object> Variables = new Dictionary<String, object>();

        public AxInterpreter(Boolean debug = false)
        {
            Debug = debug;

            //Get static functions.
            SharpFunctions = AxSharpFunctions.AddBaseFunctions();

            //Instance based functions
            RegisterOwnFunction("set", "Set");
            RegisterOwnFunction("get", "TryGetVar");
            RegisterOwnFunction("if", "If");
            RegisterOwnFunction("while", "While");
            RegisterOwnFunction("do", "Do");
            RegisterOwnFunction("ifelse", "IfElse");
            RegisterOwnFunction("for", "For");
            RegisterOwnFunction("or", "Or");
            RegisterOwnFunction("and", "And");
            RegisterOwnFunction("not", "Not");
            RegisterOwnFunction("goto", "GotoLabel");
            RegisterOwnFunction("array", "Array");
            RegisterOwnFunction("run", "RunString");
            RegisterOwnFunction("dofile", "RunFile");
        }

        #region Dynamic Functions

        private void RegisterOwnFunction(string name, string funcName)
        {
            RegisterFunction(name, new NetFunction(GetType().GetMethod(funcName), this));
        }

        public void GotoLabel(string func, string name)
        {
            var axFunc = Functions[func];

            foreach (var a in axFunc.Tags.Where(a => a.Key == "Label").Where(a => a.Value.Item1 == name))
            {
                Console.WriteLine("go to: {0}:{1}", func, a.Value.Item2);
                break;
            }
        }

        public void AddStaticFunction(string axName, string func, Type type)
        {
            RegisterFunction(axName, new NetFunction(type.GetMethod(func)));
        }

        // DEPRECIATED - isset now static and works on function pointers.
        public bool DoesVarExist(String var)
        {
            return Variables.ContainsKey(var);
        }

        public object TryGetVar(AxVariablePtr var)
        {
            object output;
            var success = Variables.TryGetValue(var, out output);
            return success ? output : null;
        }

        public void If(bool cond, AxFunction then)
        {
            if (cond)
            {
                then.Call(this, null);
            }
        }

        public void IfElse(bool cond, AxFunction then, AxFunction elseF)
        {
            if (cond)
            {
                then.Call(this, null);
            }
            else
            {
                elseF.Call(this, null);
            }
        }

        public void While(dynamic cond, AxFunction Do)
        {
            if (cond is AxVariablePtr)
            {
                while (Variables[cond])
                {
                    Do.Call(this, null);
                }
            }
            else if (cond is Boolean)
            {
                throw new AxException(this, "Using a Boolean directly will mean it wont get updated, you need to use a variable pointer.");
            }
            else if (cond is AxFunction)
            {
                while ((bool) cond.Call(this, null))
                {
                    Do.Call(this, null);
                }
            }
        }

        public void Do(AxFunction Do, AxFunction cond)
        {
            do
            {
                Do.Call(this, null);
            } while ((bool) cond.Call(this, null));
        }

        public void For(object range, AxFunction func)
        {
            var r = (IEnumerable) range;
            foreach (var b in from object v in r select func.ParamCount == 1 ? new Dictionary<string, object> {{func.Parameters[0], v}} : new Dictionary<string, object>())
            {
                func.Call(this, b);
            }
        }


        public bool And(params AxFunction[] bools)
        {
            return bools.All(a => (bool) a.Call(this, null));
        }

        public bool Or(params AxFunction[] bools)
        {
            return bools.Any(a => (bool) a.Call(this, null));
        }

        public bool Not(AxFunction input)
        {
            return !(bool) input.Call(this, null);
        }

        public object RunString(string s)
        {
            return CallFuncFromString(s, new Dictionary<string, object>());
        }

        public void RunFile(String file)
        {
            Run(File.ReadAllText(file), Path.GetDirectoryName(file));
        }

        public void RegisterFunction(String name, NetFunction func)
        {
            SharpFunctions.Add(name, func);
        }

        public object Set(AxVariablePtr var, params object[] values)
        {
            if (var.Type == VariableType.Local)
            {
                throw Error("Cannot use global set to set local variable \"{0}\", please use local set instead.", var);
            }
            var nValues = values.Length == 1 ? values[0] : new List<object>(values);
            if (Variables.ContainsKey(var))
            {
                Variables[var] = nValues;
            }
            else
            {
                Variables.Add(var, nValues);
            }

            return Variables[var];
        }

        public object Array(params object[] values)
        {
            return new List<object>(values);
        }

        #endregion

        #region Interpreter Functions

        public enum VariableType
        {
            Null = -1,
            Local,
            Global,
        }

        private readonly Stack<String> _runPathStack = new Stack<string>();
        private readonly Stack<String> _scriptStack = new Stack<string>();
        private int _prefix;

        public string Prefix
        {
            get { return String.Format("{0:X4}__", _prefix); }
        }

        public void Run(string scriptText, string executionPath, bool enter = true)
        {
            _runPathStack.Push(executionPath);
            _scriptStack.Push(Script);
            _prefix++;
            AddDefault();
            var first = true;
            var looping = true;
            while (looping)
            {
                try
                {
                    if (first)
                    {
                        Script = scriptText + "\n";
                        FirstPass();
                        GetFunctions();
                        if (enter && EntryPoint == null)
                        {
                            throw new AxException(this, "No entry point found.");
                        }
                        first = false;
                    }
                    if (enter)
                    {
                        CallFunction(EntryPoint, new List<object> {"axScriptX 3"}, new Dictionary<string, object>());

                        OnScriptEnd(null);
                    }
                    looping = false;
                }
                catch (Exception e)
                {
                    if (OnScriptError(e))
                    {
                        throw;
                    }
                }
            }

            Script = _scriptStack.Pop();
            _runPathStack.Pop();
        }

        private object CallFunction(String func, List<object> parameters, Dictionary<string, object> locals)
        {
            CallStack.Add(func);

            //Class Function Call
            var colonPos = func.IndexOf(':');
            if (colonPos != -1)
            {
                var locIndex = func.IndexOf("[", 0, colonPos);
                var loc2Index = func.IndexOf(".", 0, colonPos);
                string varName;
                if (loc2Index != -1 && (locIndex == -1 || loc2Index < locIndex))
                {
                    locIndex = loc2Index;
                }
                if (locIndex != -1)
                {
                    varName = func.Substring(0, locIndex);
                }
                else
                {
                    varName = Prefix + func.Substring(0, colonPos);
                }
                var var = GetVar(varName, locals);
                var spacePos = func.IndexOf(' ', colonPos);
                if (spacePos == -1)
                {
                    spacePos = func.Length - 1;
                }
                spacePos -= colonPos;
                var Method = func.Substring(colonPos + 1, spacePos);
                ExtrapolateVariable(func, colonPos, ref var, ref locIndex);
                var paramTypes = parameters.Select(p => p.GetType()).ToArray();
                return new NetFunction(var.GetType().GetMethod(Method, paramTypes), var).Call(parameters.ToArray());
            }

            AxFunction axFunc = null;
            var call = false;
            var pprefix = true;

            if (Functions.ContainsKey(Prefix + func))
            {
                axFunc = Functions[Prefix + func];
                call = true;
            }
            else if (Variables.ContainsKey(Prefix + func))
            {
                var c = Variables[Prefix + func];
                axFunc = c as AxFunction;
                if (axFunc == null)
                {
                    return c;
                }
                call = true;
                pprefix = false;
            }
            if (call)
            {
                var paramC = axFunc.ParamCount;
                var Params = new Dictionary<String, object>();
                var rPrefix = pprefix ? Prefix : "";

                if (!axFunc.FixedParams)
                {
                    var args = new List<object>();
                    for (int i = paramC - 1; i < parameters.Count; i++)
                    {
                        args.Add(parameters[i]);
                    }
                    Params.Add(rPrefix + "params", args);
                }

                //Params.Add("return", null); // default return;
                try
                {
                    for (var i = 0; i < paramC; i++)
                    {
                        if (axFunc.Parameters[i] != "...")
                        {
                            Params.Add(rPrefix + axFunc.Parameters[i], parameters[i]);
                        }
                    }
                }
                catch (Exception)
                {
                    throw new AxException(this, "Parameter count mismatch while calling: \"" + func + "\\");
                }
                var ret = axFunc.Call(this, Params);
                return ret;
            }

            if (SharpFunctions.ContainsKey(func))
            {
                var ret = SharpFunctions[func].Call(parameters.ToArray());
                return ret;
            }

            throw Error("Function \"{0}\" not found!", func);
        }

        public AxException Error(String message, params object[] p)
        {
            if (p != null)
            {
                message = String.Format(message, p);
            }
            return new AxException(this, message);
        }

        public object CallFuncFromString(String funcString, Dictionary<String, object> Params)
        {
            var spaceIndex = funcString.IndexOf(' ');
            var oSpaceIndex = spaceIndex;
            var parameters = new List<object>();
            String funcName;
            if (spaceIndex != -1)
            {
                funcName = funcString.Substring(0, spaceIndex);

                while (spaceIndex < funcString.Length)
                {
                    int end;
                    int start;
                    oSpaceIndex = ParseParameter(funcString, Params, parameters, oSpaceIndex, out end, ref spaceIndex, out start);
                }
            }
            else
            {
                funcName = funcString;
            }

            if (funcName == "")
            {
                return null;
            }

            // HACK: Remove hacky lset.
            if (funcName == "lset")
            {
                var var = parameters[0].ToString();
                var value = parameters[1];
                var = Prefix + var;
                if (Params.ContainsKey(var))
                {
                    Params[var] = value;
                }
                else
                {
                    Params.Add(var, value);
                }

                return null;
            }
            if (funcName == "return")
            {
                var value = parameters[0];
                if (Params.ContainsKey(Prefix + "return"))
                {
                    Params[Prefix + "return"] = value;
                }
                else
                {
                    Params.Add(Prefix + "return", value);
                }

                return value;
            }
            return CallFunction(funcName, parameters, Params);
        }

        private int ParseParameter(string funcString, Dictionary<string, object> Params, List<object> parameters, int oSpaceIndex, out int end, ref int spaceIndex, out int start)
        {
            start = oSpaceIndex + 1;
            end = spaceIndex;

            while (funcString[start] == ' ' || funcString[start] == '\n')
            {
                start++;
            }

            if (funcString[start] == '"') // String
            {
                var sb = new StringBuilder();
                var j = start + 1;
                var escapeState = false;
                while (funcString[j] != '"' || escapeState)
                {
                    if (escapeState)
                    {
                        switch (funcString[j])
                        {
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case '"':
                                sb.Append('"');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case '0':
                                sb.Append('\0');
                                break;
                            case '\\':
                                sb.Append('\\');
                                break;
                            default:
                                throw new AxException(this, "Malformed escape sequence");
                        }
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
                if (end > funcString.Length)
                {
                    end = funcString.Length;
                }
            }
            else if (char.IsDigit(funcString[start]) || funcString[start] == '-' && char.IsDigit(funcString[start + 1])) // Number
            {
                end = funcString.IndexOf(" ", start);
                if (end == -1)
                {
                    end = funcString.Length;
                }
                var num = funcString.Substring(start, end - start);
                if (num.StartsWith("0x"))
                {
                    int paramInt;
                    if (int.TryParse(num.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out paramInt))
                    {
                        parameters.Add(paramInt);
                    }
                    else
                    {
                        throw Error("Malformed hex '{0}' found.", num);
                    }
                }
                else if (num.EndsWith("l"))
                {
                    long paramLong;
                    if (long.TryParse(num.Substring(0, num.Length - 1), out paramLong))
                    {
                        parameters.Add(paramLong);
                    }
                    else
                    {
                        throw Error("Malformed long '{0}' found.", num);
                    }
                }
                else if (num.EndsWith("f"))
                {
                    float paramFloat;
                    if (float.TryParse(num.Substring(0, num.Length - 1), out paramFloat))
                    {
                        parameters.Add(paramFloat);
                    }
                    else
                    {
                        throw Error("Malformed float '{0}' found.", num);
                    }
                }
                else
                {
                    if (num.Contains('.'))
                    {
                        double paramDbl;
                        if (double.TryParse(num, out paramDbl))
                        {
                            parameters.Add(paramDbl);
                        }
                        else
                        {
                            throw Error("Malformed double '{0}' found.", num);
                        }
                    }
                    else
                    {
                        int paramInt;
                        if (int.TryParse(num, out paramInt))
                        {
                            parameters.Add(paramInt);
                        }
                        else
                        {
                            throw Error("Malformed int '{0}' found.", num);
                        }
                    }
                }
            }
            else switch (funcString[start])
            {
                case '(':
                    {
                        var param = Extract(funcString.Substring(start));
                        parameters.Add(CallFuncFromString(param.Item1, Params));
                        end = start + param.Item2;
                    }
                    break;
                case '{':
                    {
                        var extr = Extract(funcString.Substring(start), '{', '}');
                        var func = extr.Item1;
                        var iparameters = new string[0];
                        var paramindex = 0;
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
                            iparameters = func.Substring(0, paramindex).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(x => Prefix + x).ToArray();
                            func = func.Substring(paramindex + 1);
                        }

                        parameters.Add(new AxFunction(iparameters, func, Prefix));
                        end = start + extr.Item2;
                    }
                    break;
                case '%':
                    {
                        var extr = Extract(funcString.Substring(start + 1), '{', '}');
                        var func = extr.Item1;
                        var iparameters = new string[0];
                        var paramindex = 0;
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
                            iparameters = func.Substring(0, paramindex).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(x => Prefix + x).ToArray();
                            func = func.Substring(paramindex + 1).TrimStart();
                        }

                        parameters.Add(new AxFunction(iparameters, String.Format("(return ({0}))", func), Prefix, true));
                        end = start + extr.Item2 + 1;
                    }
                    break;
                case '[':
                    {
                        var extr = Extract(funcString.Substring(start), '[', ']');
                        var str = extr.Item1;

                        if (str.IndexOf(',') != -1)
                        {
                            var a = str.Split(',').Select(double.Parse);
                            parameters.Add(a);
                        }
                        else if (str.IndexOf("..") != -1)
                        {
                            var rangediff = 1;
                            string[] temp;
                            if (str.IndexOf('|') != -1)
                            {
                                temp = str.Split('|');
                                rangediff = int.Parse(temp[1]);
                                str = temp[0];
                            }

                            temp = str.Split(new[] {".."}, StringSplitOptions.RemoveEmptyEntries);
                            var rangestart = int.Parse(temp[0]);
                            var rangeend = int.Parse(temp[1]);

                            var dbl = CreateRange(rangestart, rangeend, rangediff);

                            parameters.Add(dbl);
                        }

                        end = start + extr.Item2;
                    }
                    break;
                case '*':
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1)
                        {
                            end = funcString.Length;
                        }

                        var ptrfunc = funcString.Substring(start + 1, end - start - 1);

                        if (Functions.ContainsKey(Prefix + ptrfunc))
                        {
                            parameters.Add(Functions[Prefix + ptrfunc]);
                        }
                        else
                        {
                            throw Error("Invalid function refference: \"{0}\", function not found.", ptrfunc);
                        }
                    }
                    break;
                case '^':
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1)
                        {
                            end = funcString.Length;
                        }

                        string Variable = funcString.Substring(start + 1, end - start - 1);

                        var varType = VariableType.Null;
                        if (Params.ContainsKey(Variable))
                        {
                            varType = VariableType.Local;
                        }
                        else if (Variables.ContainsKey(Variable))
                        {
                            varType = VariableType.Global;
                        }

                        var varptr = new AxVariablePtr(Prefix + Variable, varType);

                        parameters.Add(varptr);
                    }
                    break;
                case '@':
                    {
                        start++;
                        string varName;
                        end = funcString.IndexOf(" ", start);
                        if (end == -1)
                        {
                            end = funcString.Length;
                        }
                        var locindex = funcString.IndexOf("[", start, end - start);
                        var loc2index = funcString.IndexOf(".", start + 1, end - start - 1);
                        if (loc2index != -1 && loc2index < locindex)
                        {
                            locindex = loc2index;
                        }
                        varName = locindex != -1 ? funcString.Substring(start, locindex - start) : funcString.Substring(start, end - start);
                        varName = Prefix + varName;
                        var var = GetVar(varName, Params);
                        ExtrapolateVariable(funcString, end, ref var, ref locindex);

                        parameters.Add(var);
                    }
                    break;
                case '$':
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1)
                        {
                            end = funcString.Length;
                        }
                        var locindex = funcString.IndexOf("[", start + 1, end - start - 1);
                        var loc2index = funcString.IndexOf(".", start + 1, end - start - 1);
                        if (loc2index != -1 && loc2index < locindex)
                        {
                            locindex = loc2index;
                        }
                        var varName = locindex != -1 ? funcString.Substring(start + 1, locindex - start - 1) : funcString.Substring(start + 1, end - start - 1);


                        var var = Variables[Prefix + varName];
                        ExtrapolateVariable(funcString, end, ref var, ref locindex);

                        parameters.Add(var);
                    }
                    break;
                case '#':
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1)
                        {
                            end = funcString.Length;
                        }
                        var locindex = funcString.IndexOf("[", start, end - start);
                        var loc2index = funcString.IndexOf(".", start + 1, end - start - 1);

                        if (loc2index != -1 && (loc2index < locindex || locindex == -1))
                        {
                            locindex = loc2index;
                        }
                        var varName = locindex != -1 ? funcString.Substring(start + 1, locindex - start - 1) : funcString.Substring(start + 1, end - start - 1);
                        var Var = GetVar(varName, Params);
                        ExtrapolateVariable(funcString, end, ref Var, ref locindex);

                        parameters.Add(Var);
                    }
                    break;
                default:
                    {
                        end = funcString.IndexOf(" ", start);
                        if (end == -1)
                        {
                            end = funcString.Length;
                        }
                        var locindex = funcString.IndexOf("[", start, end - start);
                        var loc2index = funcString.IndexOf(".", start + 1, end - start - 1);

                        if (loc2index != -1 && (loc2index < locindex || locindex == -1))
                        {
                            locindex = loc2index;
                        }
                        var varName = locindex != -1 ? funcString.Substring(start, locindex - start) : funcString.Substring(start, end - start);
                        varName = Prefix + varName;
                        var var = GetVar(varName, Params);
                        ExtrapolateVariable(funcString, end, ref var, ref locindex);

                        parameters.Add(var);
                    }
                    break;
            }


            oSpaceIndex = end;

            spaceIndex = end + 1;
            return oSpaceIndex;
        }

        // Checks for element indexers, and loops through them all to get to the data desired.
        private void ExtrapolateVariable(String funcString, int end, ref object Var, ref int locindex)
        {
            if (locindex == -1)
            {
                return;
            }

            while (locindex < end)
            {
                var c = funcString[locindex];
                if (c == '[')
                {
                    var locIndexEnd = funcString.IndexOf("]", locindex + 1, end - locindex - 1);
                    var indexerStr = funcString.Substring(locindex + 1, locIndexEnd - locindex - 1);
                    var indexer = int.Parse(indexerStr);
                    var Enum = ((IEnumerable) Var).GetEnumerator();
                    Enum.MoveNext();

                    for (var i = 0; i < indexer; i++)
                    {
                        if (!Enum.MoveNext())
                        {
                            throw Error("Invalid indexer specified, it is larger than the collection.");
                        }
                    }

                    Var = Enum.Current;

                    Enum.Reset();

                    locindex = locIndexEnd + 1;
                }
                else if (c == '.')
                {
                    var locIndexEnd = funcString.IndexOf("[", locindex + 1, end - locindex - 1);
                    var locIndexEnd2 = funcString.IndexOf(".", locindex + 1, end - locindex - 1);
                    if (locIndexEnd2 != -1 && locIndexEnd2 < locIndexEnd)
                    {
                        locIndexEnd = locIndexEnd2;
                    }
                    if (locIndexEnd == -1)
                    {
                        locIndexEnd = end; //hit the end
                    }
                    var indexerStr = funcString.Substring(locindex + 1, locIndexEnd - locindex - 1);
                    var t = Var.GetType();

                    var prop = t.GetField(indexerStr) ?? (dynamic) t.GetProperty(indexerStr);

                    Var = prop.GetValue(Var);
                    locindex = locIndexEnd;
                }
                else
                {
                    break;
                }
            }
        }

        private IEnumerable<int> CreateRange(int start, int end, int difference = 1)
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
                if (difference >= 0)
                {
                    throw Error("Infinite range found: [{0}..{1}|{2}]", start, end, difference);
                }
                while (start >= end)
                {
                    nums.Add(start);
                    start += difference;
                }
            }
            else
            {
                if (difference <= 0)
                {
                    throw Error("Infinite range found: [{0}..{1}|{2}]", start, end, difference);
                }
                while (start <= end)
                {
                    nums.Add(start);
                    start += difference;
                }
            }

            return nums;
        }

        private object GetVar(string variable, Dictionary<string, object> local)
        {
            var varType = VariableType.Null;
            if (local.ContainsKey(variable))
            {
                varType = VariableType.Local;
            }
            else if (Variables.ContainsKey(variable))
            {
                varType = VariableType.Global;
            }
            dynamic Var;
            switch (varType)
            {
                case VariableType.Global:
                    Var = Variables[variable];
                    break;
                case VariableType.Local:
                    Var = local[variable];
                    break;
                default:
                    throw new AxException(this, String.Format("Variable \"{0}\" not found.", variable));
            }

            return Var;
        }

        private void GetFunctions()
        {
            for (var i = 0; i < Script.Length; i++)
            {
                if (Script[i] == '~' && Script[i + 1] == '(')
                {
                    var func = Extract(Script.Substring(i));
                    if (func.Item2 != -1)
                    {
                        var functionString = func.Item1;

                        var indexOfColon = functionString.IndexOf(':');
                        var indexOfParenthesis = functionString.IndexOf('(');
                        var indexOfPeriod = functionString.IndexOf('|');

                        var functionName = functionString.Substring(0, indexOfColon);
                        //Console.WriteLine (FunctionName);
                        string[] functionParameters;
                        string functionContents;
                        if (indexOfPeriod != -1 && indexOfParenthesis > indexOfPeriod)
                        {
                            functionParameters = functionString.Substring(indexOfColon + 1, indexOfPeriod - indexOfColon - 1).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(x => Prefix + x).ToArray();
                            functionContents = functionString.Substring(indexOfPeriod + 1);
                        }
                        else
                        {
                            functionParameters = new string[0];
                            functionContents = functionString.Substring(indexOfParenthesis);
                        }
                        var functionFixedParams = !functionParameters.Contains("...");
                        var axFunc = new AxFunction(functionParameters, functionContents, Prefix, functionFixedParams);
                        axFunc.GetDynamicTags();
                        Functions.Add(Prefix + functionName, axFunc);
                        if (axFunc.Tags.ContainsKey("EntryPoint"))
                        {
                            EntryPoint = functionName;
                        }

                        foreach (var t in axFunc.Tags.Where(t => Hooks.ContainsKey(t.Key)))
                        {
                            Hooks[t.Key].Call(this, axFunc);
                        }

                        i += func.Item2;
                    }
                    else
                    {
                        Console.WriteLine("Invalid function");
                    }
                }
            }
        }

        #endregion

        #region Helper Functions

        public static Tuple<string, int> Extract(string Script, char startblock = '(', char endblock = ')', bool escapeCounts = false)
        {
            //Add one to par each time we hit a startblock
            //Take one from par each time we hit an endblock
            //Therefore when it gets to 0 we reach the endblock our startblock relates to.
            var par = 1;
            var i = 0; //substring start
            var j = 0; //substring end

            while (i < Script.Length && Script[i] != startblock)
            {
                i++;
            }

            if (i != Script.Length)
            {
                while (par > 0)
                {
                    j++;
                    if (Script[j + i] == endblock)
                    {
                        par--;
                    }
                    else if (Script[j + i] == startblock)
                    {
                        par++;
                    }
                }
                return new Tuple<string, int>(Script.Substring(i + 1, j - 1), j);
            }
            else
            {
                return new Tuple<string, int>("", -1);
            }
        }

        private void FirstPass()
        {
            var sb = new StringBuilder();
            var inStr = false;
            var inComment = false;
            var inCompStatement = false;
            var compStateBuffer = new StringBuilder();
            for (var i = 0; i < Script.Length - 1; i++)
            {
                if (Script[i] == '\n')
                {
                    inComment = false;
                    if (inCompStatement)
                    {
                        inCompStatement = false;
                        var str = compStateBuffer.ToString();
                        var split = str.IndexOf(' ');
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
                    if (Script[i + 1] == '"')
                    {
                        if (Script[i] != '/')
                        {
                            inStr = !inStr;
                        }
                    }
                    if (!inStr)
                    {
                        //Remove Comments
                        if (Script[i] == '/')
                        {
                            if (Script[i + 1] == '/')
                            {
                                inComment = true;
                                continue;
                            }
                        }
                        //Remove and store Compiler Statements
                        if (Script[i] == '#' && (i == 0 || Script[i - 1] == '\n'))
                        {
                            inCompStatement = true;
                            continue;
                        }
                    }

                    if (inCompStatement && Script[i] != '\r')
                    {
                        compStateBuffer.Append(Script[i]);
                        continue;
                    }

                    if (!(IsNewLine(Script[i]) || Script[i] == '\t' || (i > 0 && Script[i] == ' ' && Script[i - 1] == ' ')))
                    {
                        sb.Append(Script[i]);
                    }
                }
            }

            Script = sb.ToString();
        }

        private void ParseCompilerStatement(string statement, string p)
        {
            statement = statement.ToLower();
            string f;
            switch (statement)
            {
                case "module":
                    f = ConvertToRealPath(p);
                    AxModuleLoader.Load(this, f);
                    break;
                case "include":
                    f = ConvertToRealPath(p);
                    Run(File.ReadAllText(Path.GetFullPath(f)), Path.GetDirectoryName(f), false);
                    break;
            }
        }

        public string ConvertToRealPath(String inp)
        {
            if (inp[0] == '"' || inp[0] == '<')
            {
                var f = inp.Substring(1, inp.Length - 2);

                if (inp[0] == '"')
                {
                    f = Path.Combine(_runPathStack.Peek(), f);
                }
                return f;
            }
            throw new AxException(this, "Invalid path specified. You need to encapsulate in quotations (relative) or angle brackets (absolute).");
        }

        public static bool IsNewLine(char c)
        {
            return c == '\n' || c == '\r';
        }

        #endregion

        #region Overrides

        public object this[string varName]
        {
            get { return Variables[Prefix + varName]; }
            set { Variables[Prefix + varName] = value; }
        }

        #endregion

        public event ScriptEndEvent ScriptError;

        private bool OnScriptError(Exception e)
        {
            return ScriptError == null || ScriptError(e);
        }

        public event ScriptEndEvent ScriptEnd;

        private bool OnScriptEnd(Exception e)
        {
            return ScriptEnd == null || ScriptEnd(e);
        }

        private void AddDefault()
        {
            Variables.Add(Prefix + "null", null);
            Variables.Add(Prefix + "true", true);
            Variables.Add(Prefix + "false", false);
        }

        public void RegisterHook(string tag, NetFunction netFunction)
        {
            Hooks.Add(tag, netFunction);
        }
    }
}