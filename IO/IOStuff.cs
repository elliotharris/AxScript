using System;
using axScript3;

namespace IO
{
    public class IOStuff
    {
        [ExportAx("__IOVERSION", "Version of the IO Library")] public static int IOVersion = 200;

        [ExportAx("printl", "Prints a line of text.")]
        public static void PrintLine(object Format, params object[] Parameters)
        {
            if (Format.GetType() == typeof(String))
                Console.WriteLine((string)Format, Parameters);
            else
                Console.WriteLine(Format);
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
            return input is string ? (byte) input[0] : (byte) input;
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
