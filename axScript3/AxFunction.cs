using System;
using System.Collections.Generic;
using System.Text;

namespace axScript3
{
    public class AxFunction
    {
        public bool FixedParams;
        public string InnerFunction;
        public int ParamCount;
        public string[] Parameters;
        public string Prefix;
        public Dictionary<String, Tuple<String, int>> Tags = new Dictionary<String, Tuple<String, int>>();

        // gets ~[ ] declarations.

        public AxFunction(string[] parameters, string function, string prefix, bool fixedParams = true)
        {
            Parameters = parameters;
            InnerFunction = function;
            ParamCount = parameters.Length;
            FixedParams = fixedParams;
            Prefix = prefix;
        }

        public void GetDynamicTags()
        {
            for (int i = InnerFunction.Length - 1; i > 0; i--)
            {
                if (InnerFunction[i] == '[' && InnerFunction[i - 1] == '~')
                {
                    Tuple<string, int> extr = AxInterpreter.Extract(InnerFunction.Substring(i), '[', ']');
                    string Tag = extr.Item1;
                    int Length = extr.Item2;
                    //Console.WriteLine (Tag);
                    InnerFunction = InnerFunction.Remove(i - 1, Length + 2);

                    var Keys = new List<String>();
                    Keys.AddRange(Tags.Keys);
                    foreach (string a in Keys)
                    {
                        Tags[a] = new Tuple<string, int>(Tags[a].Item1, Tags[a].Item2 - Length - 2);
                    }

                    int spaceIndex = Tag.IndexOf(' ');
                    if (spaceIndex == -1)
                    {
                        Tags.Add(Tag, new Tuple<String, int>("", i));
                    }
                    else
                    {
                        Tags.Add(Tag.Substring(0, spaceIndex), new Tuple<String, int>(Tag.Substring(spaceIndex + 1, Tag.Length - spaceIndex - 1), i));
                    }
                }
            }

            Console.WriteLine();
        }

        public override string ToString()
        {
            var a = new StringBuilder("Func: ");
            a.Append(InnerFunction);
            a.Append(" | Params: ");
            foreach (string b in Parameters)
            {
                a.Append(b);
                a.Append(" ");
            }

            if (Parameters.Length == 0)
            {
                a.Append("(NULL)");
            }

            return a.ToString();
        }

        public T Call<T>(AxInterpreter caller, object[] Params)
        {
            var rps = new Dictionary<string, object>();
            for (int i = 0; i < Parameters.Length; i++)
            {
                rps.Add(Parameters[i], Params[i]);
            }

            return Call<T>(caller, rps);
        }

        public T Call<T>(AxInterpreter caller, Dictionary<String, object> Params)
        {
            object r = Call(caller, Params);
            return (T) r;
        }

        public object Call(AxInterpreter caller, Dictionary<String, object> Params)
        {
            if (Params == null)
            {
                Params = new Dictionary<string, object>();
            }
            bool done = false;
            for (int i = 0; i < InnerFunction.Length; i++)
            {
                Tuple<string, int> inner = AxInterpreter.Extract(InnerFunction.Substring(i));

                int offset = 0;
                while (inner.Item1.StartsWith("("))
                {
                    inner = AxInterpreter.Extract(inner.Item1);

                    //offset += 2;
                }
                if (inner.Item2 < 0)
                {
                    done = true;
                }
                if (done)
                {
                    break;
                }

                i += inner.Item2 + offset;
                caller.CallFuncFromString(inner.Item1, Params);
            }

            if (Params.ContainsKey(caller.Prefix + "return"))
            {
                return Params[caller.Prefix + "return"];
            }
            else
            {
                return null;
            }
        }
    }
}