using System;
using System.Collections.Generic;

namespace axScript3
{
    public static class AxSharpFunctions
    {
        public static Dictionary<String, NetFunction> AddBaseFunctions()
        {
            var funcs = new Dictionary<String, NetFunction>
                {
                    {"add", GetFunc("Add")}, 
                    {"+", GetFunc("Add")}, 
                    {"sub", GetFunc("Subtract")}, 
                    {"-", GetFunc("Subtract")}, 
                    {"div", GetFunc("Divide")}, 
                    {"/", GetFunc("Divide")}, 
                    {"mult", GetFunc("Multiply")}, 
                    {"*", GetFunc("Multiply")}, 
                    {"double", GetFunc("ToDouble")}, 
                    {"int", GetFunc("ToInt")}, 
                    {"lt", GetFunc("LessThan")}, 
                    {"<", GetFunc("LessThan")}, 
                    {"gt", GetFunc("GreaterThan")}, 
                    {">", GetFunc("GreaterThan")}, 
                    {"eq", GetFunc("EqualTo")}, 
                    {"==", GetFunc("EqualTo")}, 
                    {"neq", GetFunc("NotEqualTo")}, 
                    {"!=", GetFunc("NotEqualTo")}, 
                    {"scope", GetFunc("ReturnScope")}, 
                    {"isset", GetFunc("IsPointerValid")}, 
                    {"??", GetFunc("IsPointerValid")}, 
                    {"type", GetFunc("TypeOf")}
                };

            return funcs;
        }

        private static NetFunction GetFunc(string f)
        {
            return new NetFunction(typeof (AxSharpFunctions).GetMethod(f));
        }

        #region Static Functions

        #region Maths

        public static dynamic Add(dynamic a, params dynamic[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                a += parameters[i];
            }

            return a;
        }


        public static dynamic Subtract(dynamic a, params dynamic[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                a -= parameters[i];
            }

            return a;
        }

        public static dynamic Multiply(dynamic a, params dynamic[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                a *= parameters[i];
            }

            return a;
        }

        public static dynamic Divide(dynamic a, params dynamic[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                a /= parameters[i];
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
            return ptr.Type != AxInterpreter.VariableType.Null;
        }

        #endregion

        #region Arrays

        public static void ArrayInsert(List<object> Array, params object[] Values)
        {
            Array.AddRange(Values);
        }

        public static int ArrayCount(object inp)
        {
            return inp is string ? ((String) inp).Length : ((List<object>) inp).Count;
        }

        public static void ArrayRemove(List<object> Array, params object[] Values)
        {
            foreach (var a in Values)
            {
                Array.Remove(a);
            }
        }

        public static void ArrayRemoveAt(List<object> Array, params double[] Indexes)
        {
            foreach (var i in Indexes)
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

        #endregion

        #endregion
    }
}