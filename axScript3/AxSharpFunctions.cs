using System;
using System.Collections.Generic;

namespace axScript3
{
	public static class AxSharpFunctions
	{
		public static Dictionary<String, NetFunction> AddBaseFunctions ()
		{
			Dictionary<String, NetFunction> Funcs = new Dictionary<String, NetFunction>();
			
			//Funcs.Add("print", GetFunc("PrintLine"));
           // Funcs.Add("mprint", GetFunc("Print"));
			Funcs.Add("add", GetFunc("Add"));
			Funcs.Add("sub", GetFunc("Subtract"));
			Funcs.Add("div", GetFunc("Divide"));
			Funcs.Add("mult", GetFunc("Multiply"));
			Funcs.Add("and", GetFunc("And"));
			Funcs.Add("or", GetFunc("Or"));
            Funcs.Add("not", GetFunc("Not"));
			//Funcs.Add("read", GetFunc("ReadIn"));
            //Funcs.Add("readkey", GetFunc("ReadKey"));
			Funcs.Add("str2num", GetFunc("StrToNum"));
			Funcs.Add("length", GetFunc("ArrayCount"));
			Funcs.Add("insert", GetFunc("ArrayInsert"));
			Funcs.Add("remove", GetFunc("ArrayRemove"));
			Funcs.Add("removeAt", GetFunc("ArrayRemoveAt"));
			//Funcs.Add("print+", GetFunc("_Print"));
			Funcs.Add("toString", GetFunc("_ToString"));
			Funcs.Add("rset", GetFunc("RefSet"));
			Funcs.Add("lt", GetFunc("LessThan"));
			Funcs.Add("gt", GetFunc("GreaterThan"));
            Funcs.Add("eq", GetFunc("EqualTo"));
            Funcs.Add("scope", GetFunc("ReturnScope"));
            Funcs.Add("isset", GetFunc("IsPointerValid"));
			return Funcs;
		}
		
		static NetFunction GetFunc(string f)
		{
			return new NetFunction(typeof(AxSharpFunctions).GetMethod(f));	
		}
		
		#region Static Functions
		
		#region Maths
		public static double Add(double a, params double[] Parameters)
		{
			for(int i = 0; i < Parameters.Length; i++)
			{
				a += Parameters[i];
			}
			
			return a;
		}
		
		public static double Subtract(double a, params double[] Parameters)
		{
			for(int i = 0; i < Parameters.Length; i++)
			{
				a -= Parameters[i];
			}
			
			return a;
		}
		
		public static double Multiply(double a, params double[] Parameters)
		{
			for(int i = 0; i < Parameters.Length; i++)
			{
				a *= Parameters[i];
			}
			
			return a;
		}
		
		public static double Divide(double a, params double[] Parameters)
		{
			for(int i = 0; i < Parameters.Length; i++)
			{
				a /= Parameters[i];
			}
			
			return a;
		}
		#endregion
		
		#region Logic
		public static bool LessThan(double a, double b)
		{
			return a < b;	
		}
		
		public static bool GreaterThan(double a, double b)
		{
			return a > b;	
		}
		
		public static bool EqualTo(object a, object b)
		{
			return a.Equals(b);	
		}
		
		public static int Compare<T>(T a, T b) where T : IComparable
		{
			return a.CompareTo(b);
		}
		
		public static bool And(params bool[] Bools)
		{
			foreach(bool a in Bools) if(!a) return false;
			return true;
		}
		
		public static bool Or(params bool[] Bools)
		{
			foreach(bool a in Bools) if(a) return true;
			return false;
		}

        public static bool Not(bool input)
        {
            return !input;
        }
		#endregion

        #region Pointers
        public static string ReturnScope(AxVariablePtr ptr)
        {
            switch (ptr.Type)
            {
                case AxInterpreter.VariableType.Null:
                    return "NULL";
                case AxInterpreter.VariableType.Local:
                    return "Local";
                case AxInterpreter.VariableType.Global:
                    return "Global";
                default:
                    return null;
            }

        }

        public static bool IsPointerValid(AxVariablePtr ptr)
        {
            return ptr.Type == AxInterpreter.VariableType.Null ? false : true;
        }
        #endregion
		
		#region Arrays
		public static void ArrayInsert(List<object> Array, params object[] Values)
		{
			Array.AddRange(Values);
		}
		
		public static int ArrayCount(object inp)
		{
			if(inp.GetType() == typeof(String))
			{
				return ((String)inp).Length;
			}
			else
			{
				return ((List<object>)inp).Count;	
			}
		}
		
		public static void ArrayRemove(List<object> Array, params object[] Values)
		{
			foreach(object a in Values)
			{
				Array.Remove(a);	
			}
		}
		
		public static void ArrayRemoveAt(List<object> Array, params double[] Indexes)
		{
			foreach(double i in Indexes)
			{
				Array.RemoveAt(Convert.ToInt32(i));	
			}
		}
		
		public static void RefSet(out object input, object Value)
		{
			input = Value;
		}
		#endregion
		
		#region Conversion
		public static double StrToNum(string num)
		{
			return double.Parse(num);
		}
		
		public static string _ToString(object inp)
		{
			return inp.AdvToString();	
		}
		#endregion
		
		#endregion
	}
}

