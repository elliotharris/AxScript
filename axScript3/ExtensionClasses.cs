using System;
using System.Collections.Generic; 
using System.Collections;
using System.Text;

namespace axScript3
{
	public static class ExtensionClasses
	{
		public static void Print<T>(this T a)
		{
			Console.WriteLine (a.AdvToString());
		}

        public static int ToInt(this double d)
        {
            return Convert.ToInt32(d);
        }
		
		public static String AdvToString<T>(this T a)
		{
			StringBuilder SB = new StringBuilder();
			if(a is IEnumerable && !(a is String))
			{
				SB.Append("{ ");
				foreach(var b in (IEnumerable)a)
				{
					SB.Append(String.Format("{0} ", b.AdvToString()));	
				}
				SB.Append("}");
			}
			else
				
			{
				SB.Append(a);
			}
			
			return SB.ToString();
		}
	}
}

