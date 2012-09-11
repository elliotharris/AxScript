using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace axScript3
{
    public class NetFunction
    {
        public readonly bool HasParamArray;
        public ParameterInfo[] Inputs;
        public MethodBase Method;
        public Type Output;
        public object Target;

        /// <summary>
        /// Initializes a new instance of the <see cref="axScript3.NetFunction"/> class.
        /// </summary>
        /// <param name='function'>
        /// Function.
        /// </param>
        /// <param name='target'>
        /// Owner of function, leave null if static function.
        /// </param>
        public NetFunction(MethodBase function, object target = null)
        {
            Method = function;
            Inputs = function.GetParameters();
            Output = function.GetType();
            Target = target;
            HasParamArray = Inputs.Length > 0 && Inputs[Inputs.Length - 1].GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;
        }

        public object Call(params object[] input)
        {
            try
            {
                if (HasParamArray)
                {
                    int lastParamPosition = Inputs.Length - 1;

                    var realParams = new object[Inputs.Length];
                    for (int i = 0; i < lastParamPosition; i++)
                    {
                        realParams[i] = input[i];
                    }

                    Type paramsType = Inputs[lastParamPosition].ParameterType.GetElementType();
                    Array extra = Array.CreateInstance(paramsType, input.Length - lastParamPosition);
                    for (int i = 0; i < extra.Length; i++)
                    {
                        extra.SetValue(input[i + lastParamPosition], i);
                    }

                    realParams[lastParamPosition] = extra;

                    input = realParams;
                }

                // Add in defaults
                if (input.Length < Inputs.Length)
                {
                    IEnumerable<ParameterInfo> optInputs = from a in Inputs where a.IsOptional select a;
                    int i = Inputs.Length - input.Length;
                    List<object> inps = input.ToList();
                    foreach (ParameterInfo b in optInputs)
                    {
                        if (i > 0)
                        {
                            i++;
                            inps.Add(b.DefaultValue);
                        }
                        else
                        {
                            break;
                        }
                    }
                    input = inps.ToArray();
                }

                return Method.Invoke(Target, input);
            }
            catch (Exception e)
            {
                throw new Exception("Cannot complete call to C# function: \"" + Method.Name + "\", " + e.Message, e);
            }
        }
    }
}