using System;
using System.Reflection;

namespace axScript3
{
	public class NetFunction
	{
		public MethodBase Method;
		public ParameterInfo[] Inputs;
		public Type Output;
		public object Target;
		bool hasParams;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="axScript3.NetFunction"/> class.
		/// </summary>
		/// <param name='function'>
		/// Function.
		/// </param>
		/// <param name='target'>
		/// Owner of function, leave null if static function.
		/// </param>
		public NetFunction (MethodBase function, object target = null)
		{
			Method = function;
			Inputs = function.GetParameters();
			Output = function.GetType();
			this.Target = target;
			hasParams = Inputs.Length > 0 && Inputs[Inputs.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
		}
		
		public object call(params object[] input)
		{		
		    if (hasParams)
		    {
		        int lastParamPosition = Inputs.Length - 1;
		
		        object[] realParams = new object[Inputs.Length];
		        for (int i = 0; i < lastParamPosition; i++)
		            realParams[i] = input[i];
		
		        Type paramsType = Inputs[lastParamPosition].ParameterType.GetElementType();
		        Array extra = Array.CreateInstance(paramsType, input.Length - lastParamPosition);
		        for (int i = 0; i < extra.Length; i++)
		            extra.SetValue(input[i + lastParamPosition], i);
		
		        realParams[lastParamPosition] = extra;
		
		        input = realParams;	
		    }
			
			return Method.Invoke(Target, input);
		}
	}
}

