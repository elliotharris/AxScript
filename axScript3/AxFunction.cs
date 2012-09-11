using System;
using System.Collections.Generic;
using System.Text;

namespace axScript3
{
    public class AxFunction
    {
        public readonly bool FixedParams;
        public string InnerFunction;
        public readonly int ParamCount;
        public readonly string[] Parameters;
        public readonly string Prefix;
        public readonly Dictionary<String, Tuple<String, int>> Tags = new Dictionary<String, Tuple<String, int>>();

        public AxFunction(string[] parameters, string function, string prefix, bool fixedParams = true)
        {
            Parameters = parameters;
            InnerFunction = function;
            ParamCount = parameters.Length;
            FixedParams = fixedParams;
            Prefix = prefix;
        }

        /// <summary>
        /// Populates Tag array with all ~[] tags in the function body.
        /// </summary>
        public void GetDynamicTags()
        {
            for (var i = InnerFunction.Length - 1; i > 0; i--)
            {
                if (InnerFunction[i] == '[' && InnerFunction[i - 1] == '~')
                {
                    var extr = AxInterpreter.Extract(InnerFunction.Substring(i), '[', ']');
                    var tag = extr.Item1;
                    var length = extr.Item2;
                    //Console.WriteLine (Tag);
                    InnerFunction = InnerFunction.Remove(i - 1, length + 2);

                    var keys = new List<String>();
                    keys.AddRange(Tags.Keys);
                    foreach (var a in keys)
                    {
                        Tags[a] = new Tuple<string, int>(Tags[a].Item1, Tags[a].Item2 - length - 2);
                    }

                    var spaceIndex = tag.IndexOf(' ');
                    if (spaceIndex == -1)
                    {
                        Tags.Add(tag, new Tuple<String, int>("", i));
                    }
                    else
                    {
                        Tags.Add(tag.Substring(0, spaceIndex), new Tuple<String, int>(tag.Substring(spaceIndex + 1, tag.Length - spaceIndex - 1), i));
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
            foreach (var b in Parameters)
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
            for (var i = 0; i < Parameters.Length; i++)
            {
                rps.Add(Parameters[i], Params[i]);
            }

            return Call<T>(caller, rps);
        }

        public T Call<T>(AxInterpreter caller, Dictionary<String, object> Params)
        {
            var r = Call(caller, Params);
            if (r == null) return default(T);
            return (T) r;
        }

        public object Call(AxInterpreter caller, Dictionary<String, object> Params)
        {
            if (Params == null)
            {
                Params = new Dictionary<string, object>();
            }
            var done = false;
            for (var i = 0; i < InnerFunction.Length; i++)
            {
                var inner = AxInterpreter.Extract(InnerFunction.Substring(i));
                while (inner.Item1.StartsWith("("))
                {
                    inner = AxInterpreter.Extract(inner.Item1);
                }
                if (inner.Item2 < 0)
                {
                    done = true;
                }
                if (done)
                {
                    break;
                }

                i += inner.Item2;
                caller.CallFuncFromString(inner.Item1, Params);
            }

            return Params.ContainsKey(caller.Prefix + "return") ? Params[caller.Prefix + "return"] : null;
        }
    }
}