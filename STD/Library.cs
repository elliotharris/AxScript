using System;
using System.Collections.Generic;
using axScript3;

namespace STD
{
    public static class Library
    {
        [ExportAx("insert", "Inserts values into an array.")]
        public static void ArrayInsert(List<object> array, params object[] values)
        {
            array.AddRange(values);
        }

        [ExportAx("length", "Returns the length of an array or string.")]
        public static int ArrayCount(object inp)
        {
            if (inp is string)
            {
                return ((String)inp).Length;
            }
            return ((List<object>)inp).Count;
        }

        [ExportAx("remove", "Removes values from an array.")]
        public static void ArrayRemove(List<object> array, params object[] values)
        {
            foreach (var a in values)
            {
                array.Remove(a);
            }
        }

        [ExportAx("removeat", "Removes the values at the indexes specified.")]
        public static void ArrayRemoveAt(List<object> array, params int[] indexes)
        {
            foreach (var i in indexes)
            {
                array.RemoveAt(i);
            }
        }

        [ExportAx("printl", "Prints a line of text.")]
        public static void PrintLine(object format, params object[] parameters)
        {
            if (format is string)
                Console.WriteLine((string)format, parameters);
            else
                Console.WriteLine(format);
        }

        [ExportAx("readkey", "Reads in a single keypress")]
        public static double ReadKey(bool hide = false)
        {
            var c = Console.ReadKey(hide).KeyChar;
            return c;
        }

        [ExportAx("byte", "Converts a char to a byte.")]
        public static byte GetByte(dynamic input)
        {
            return input is string ? (byte)input[0] : (byte)input;
        }

        [ExportAx("read", "Reads in a line.")]
        public static string ReadIn(string prompt = "")
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        [ExportAx("print", "Prints out text without creating a newline.")]
        public static void Print(object format, params object[] parameters)
        {
            if (format is string)
                Console.Write((string)format, parameters);
            else
                Console.Write(format);
        }

        [ExportAx("printarr", "Prints out all values of an enumerable")]
        public static void PrintArray(object var)
        {
            Console.WriteLine("{0}", var.AdvToString());
        }
    }
}
