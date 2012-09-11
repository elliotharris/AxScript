using System;
using System.Collections.Generic;

namespace axScript3
{
    public static class AxSharpFunctions
    {
        public static Dictionary<String, NetFunction> AddBaseFunctions()
        {
            var funcs = new Dictionary<String, NetFunction>();

            funcs.Add("add", GetFunc("Add"));
            funcs.Add("+", GetFunc("Add"));
            funcs.Add("sub", GetFunc("Subtract"));
            funcs.Add("-", GetFunc("Subtract"));
            funcs.Add("div", GetFunc("Divide"));
            funcs.Add("/", GetFunc("Divide"));
            funcs.Add("mult", GetFunc("Multiply"));
            funcs.Add("*", GetFunc("Multiply"));
            funcs.Add("double", GetFunc("ToDouble"));
            funcs.Add("int", GetFunc("ToInt"));
            funcs.Add("string", GetFunc("_ToString"));
            funcs.Add("lt", GetFunc("LessThan"));
            funcs.Add("<", GetFunc("LessThan"));
            funcs.Add("gt", GetFunc("GreaterThan"));
            funcs.Add(">", GetFunc("LessThan"));
            funcs.Add("eq", GetFunc("EqualTo"));
            funcs.Add("==", GetFunc("LessThan"));
            funcs.Add("neq", GetFunc("NotEqualTo"));
            funcs.Add("!=", GetFunc("LessThan"));
            funcs.Add("scope", GetFunc("ReturnScope"));
            funcs.Add("isset", GetFunc("IsPointerValid"));
            funcs.Add("??", GetFunc("IsPointerValid"));
            funcs.Add("type", GetFunc("TypeOf"));

            return funcs;
        }

        private static NetFunction GetFunc(string f)
        {
            return new NetFunction(typeof (AxSharpFunctions).GetMethod(f));
        }

        #region Static Functions

        #region Maths

        public static dynamic Add(dynamic a, params dynamic[] Parameters)
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                a += Parameters[i];
            }

            return a;
        }


        public static dynamic Subtract(dynamic a, params dynamic[] Parameters)
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                a -= Parameters[i];
            }

            return a;
        }

        public static dynamic Multiply(dynamic a, params dynamic[] Parameters)
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                a *= Parameters[i];
            }

            return a;
        }

        public static dynamic Divide(dynamic a, params dynamic[] Parameters)
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                a /= Parameters[i];
            }

            return a;
        }

        #endregion

        #region Logic

        public static bool LessThan(dynamic a, dynamic b)
        {
            return a < b;
        }

        public static bool GreaterThan(dynamic a, dynamic b)
        {
            return a > b;
        }

        public static bool EqualTo(dynamic a, dynamic b)
        {
            return a == b;
        }

        public static bool NotEqualTo(dynamic a, dynamic b)
        {
            return a != b;
        }

        public static int Compare<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b);
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

        public static Type TypeOf(object o)
        {
            return o.GetType();
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
            if (inp.GetType() == typeof (String))
            {
                return ((String) inp).Length;
            }
            else
            {
                return ((List<object>) inp).Count;
            }
        }

        public static void ArrayRemove(List<object> Array, params object[] Values)
        {
            foreach (object a in Values)
            {
                Array.Remove(a);
            }
        }

        public static void ArrayRemoveAt(List<object> Array, params double[] Indexes)
        {
            foreach (double i in Indexes)
            {
                Array.RemoveAt(Convert.ToInt32(i));
            }
        }

        #endregion

        #region Conversion

        public static int ToInt(dynamic inp)
        {
            return Convert.ToInt32(inp);
        }

        public static double ToDouble(dynamic inp)
        {
            return Convert.ToDouble(inp);
        }

        public static string _ToString(dynamic inp)
        {
            return inp.ToString();
        }

        #endregion

        #endregion
    }
}