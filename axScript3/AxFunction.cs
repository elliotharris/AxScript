using System;
using System.Text;
using System.Collections.Generic;

namespace axScript3
{
	public class AxFunction
	{
		public string[] Parameters;
		public int ParamCount;
		public string InnerFunction;
		public bool FixedParams;
		public Dictionary<String, Tuple<String, int>> Tags = new Dictionary<String, Tuple<String, int>>();
		
		// gets ~[ ] declarations.
		public void GetDynamicTags()
		{
			for(int i = InnerFunction.Length -1; i > 0; i--)
			{
				if(InnerFunction[i] == '[' && InnerFunction[i-1] == '~')
				{
					var extr = AxInterpreter.extract(InnerFunction.Substring(i), '[', ']');	
					var Tag = extr.Item1;
					var Length = extr.Item2;
					//Console.WriteLine (Tag);
					InnerFunction = InnerFunction.Remove(i-1,Length+2);
					
					List<String> Keys = new List<String>();
					Keys.AddRange(Tags.Keys);
					foreach(string a in Keys)
					{
						Tags[a] = new Tuple<string, int>(Tags[a].Item1, Tags[a].Item2-Length-2);	
					}
					
					int spaceIndex = Tag.IndexOf(' ');
					if(spaceIndex == -1)
					{
						Tags.Add(Tag, new Tuple<String, int>("", i));	
					}
					else
					{
						Tags.Add(Tag.Substring(0, spaceIndex), new Tuple<String, int>(Tag.Substring(spaceIndex+1, Tag.Length-spaceIndex-1), i));
					}
					
					
				}
			}
							
			Console.WriteLine ();				
		}
		
		public AxFunction (string[] Parameters, string Function, bool FixedParams = true)
		{
			this.Parameters = Parameters;
			this.InnerFunction = Function;
			this.ParamCount = Parameters.Length;
			this.FixedParams = FixedParams;
		}
		
		public override string ToString ()
		{
			StringBuilder a = new StringBuilder("Func: ");
			a.Append(InnerFunction);
			a.Append(" | Params: ");
			foreach(var b in Parameters)
			{
				a.Append(b);
				a.Append(" ");
			}
			
			if(Parameters.Length == 0) a.Append("(NULL)");
			
			return a.ToString();
		}
		
		public object Call(AxInterpreter caller, Dictionary<String, object> Params)
		{
			if(Params == null)
			{
				Params = new Dictionary<string, object>();
			}
			bool done = false;
			for(int i = 0; i < InnerFunction.Length; i++)
			{
				var inner = AxInterpreter.extract(InnerFunction.Substring(i));
				
				int offset = 0;
				while(inner.Item1.StartsWith("("))
				{
					inner = AxInterpreter.extract(inner.Item1);
					
					//offset += 2;
				}
				if(inner.Item2 < 0) done = true;
				if(done) break;
				
				i += inner.Item2 + offset;
				caller.callFuncFromString(inner.Item1, Params);
			}

            if (Params.ContainsKey("return"))
                return Params["return"];
            else return null;
		}
	}
}

