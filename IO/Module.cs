using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using axScript3;
using System.IO;

namespace IO
{
    public class Module : AxSharpModule
    {
        public void Load(AxInterpreter instance)
        {
            instance.RegisterFunction("read", GetFunc("ReadIn"));
            instance.RegisterFunction("readkey", GetFunc("ReadKey"));
            instance.RegisterFunction("print+", GetFunc("_Print"));
            instance.RegisterFunction("print", GetFunc("PrintLine"));
            instance.RegisterFunction("mprint", GetFunc("Print"));
        }

        static NetFunction GetFunc(string f)
        {
            return new NetFunction(typeof(Module).GetMethod(f));
        }

        public static void PrintLine(object Format, params object[] Parameters)
        {
            if (Format.GetType() == typeof(String))
                Console.WriteLine((string)Format, Parameters);
            else
                Console.WriteLine(Format);
        }

        public static double ReadKey(bool hide = false)
        {
            var c = Console.ReadKey(hide).KeyChar;
            return c;
        }

        public static string ReadIn(string Prompt = "")
        {
            Console.Write(Prompt);
            return Console.ReadLine();
        }

        public static void Print(object Format, params object[] Parameters)
        {
            if (Format.GetType() == typeof(String))
                Console.Write((string)Format, Parameters);
            else
                Console.Write(Format);
        }

        public static void _Print(object Var)
        {
            Console.WriteLine("{0}", Var.AdvToString());
        }
    }
}
