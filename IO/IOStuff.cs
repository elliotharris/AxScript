using System;
using axScript3;

namespace IO
{
    public class IOStuff
    {
        [ExportAsAxFunction("print")]
        public static void PrintLine(object Format, params object[] Parameters)
        {
            if (Format.GetType() == typeof(String))
                Console.WriteLine((string)Format, Parameters);
            else
                Console.WriteLine(Format);
        }

        [ExportAsAxFunction("readkey")]
        public static double ReadKey(bool hide = false)
        {
            var c = Console.ReadKey(hide).KeyChar;
            return c;
        }

        [ExportAsAxFunction("read")]
        public static string ReadIn(string Prompt = "")
        {
            Console.Write(Prompt);
            return Console.ReadLine();
        }

        [ExportAsAxFunction("_print")]
        public static void Print(object Format, params object[] Parameters)
        {
            if (Format.GetType() == typeof(String))
                Console.Write((string)Format, Parameters);
            else
                Console.Write(Format);
        }

        [ExportAsAxFunction("print+")]
        public static void _Print(object Var)
        {
            Console.WriteLine("{0}", Var.AdvToString());
        }
    }
}
