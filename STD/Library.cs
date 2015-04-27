using System;
using System.Collections.Generic;
using axScript3;

namespace STD
{
    public static class Library
    {
        [ExportAx("array>insert", "Inserts values into an array.")]
        public static void ArrayInsert(List<object> array, params object[] values)
        {
            array.AddRange(values);
        }

        [ExportAx("length", "Returns the length of an array or string.")]
        public static int ArrayCount(dynamic inp)
        {
            return inp is string ? ((String) inp).Length : inp.Count;
        }

        [ExportAx("array>remove", "Removes values from an array.")]
        public static void ArrayRemove(List<object> array, params object[] values)
        {
            foreach (var a in values)
            {
                array.Remove(a);
            }
        }

        [ExportAx("array>removeat", "Removes the values at the indexes specified.")]
        public static void ArrayRemoveAt(List<object> array, params int[] indexes)
        {
            foreach (var i in indexes)
            {
                array.RemoveAt(i);
            }
        }

        [ExportAx("console>clear", "Clears the console")]
        public static void ClearConsole()
        {
            Console.Clear();
        }

        [ExportAx("printl", "Prints a line of text.")]
        public static void PrintLine(object format, params object[] parameters)
        {
            if (parameters.Length == 0)
            {
                Console.WriteLine(format);
            }
            else
            {
                Console.WriteLine((string) format, parameters);
            }
        }

        [ExportAx("readkey", "Reads in a single keypress")]
        public static double ReadKey(bool hide = false)
        {
            var c = Console.ReadKey(hide).KeyChar;
            return c;
        }

        [ExportAx("convert>byte", "Converts a char to a byte.")]
        public static byte GetByte(dynamic input)
        {
            return input is string ? (byte) input[0] : (byte) input;
        }

        [ExportAx("convert>char", "Converts a byte to a char (then to a string).")]
        public static string GetChar(dynamic input)
        {
            return ((char) input).ToString();
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
            {
                Console.Write((string) format, parameters);
            }
            else
            {
                Console.Write(format);
            }
        }

        [ExportAx("array>print", "Prints out all values of an enumerable")]
        public static void PrintArray(object var)
        {
            Console.WriteLine("{0}", var.AdvToString());
        }

        [ExportAx("console>title", "Sets title of the console window")]
        public static void SetTitle(string title)
        {
            Console.Title = title;
        }
    }
}