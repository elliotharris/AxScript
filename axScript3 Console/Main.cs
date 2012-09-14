using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CommandLine;
using axScript3;

namespace axScript3Console
{
    internal static class MainClass
    {
        public static void Main(string[] args)
        {
            //try
            //{

            var consoleOptions = new ConsoleOptions();
            var parser = new CommandLineParser();
            if (!parser.ParseArguments(args, consoleOptions))
            {
                throw new Exception("Invalid options specified");
            }
            var script = File.ReadAllText(consoleOptions.Script);
            var dir = Path.GetDirectoryName(consoleOptions.Script);
            var ax = new AxInterpreter(consoleOptions.Debug);

            if (consoleOptions.Debug) Console.WriteLine("axScript {0}", Assembly.GetExecutingAssembly().GetName().Version);
            if (consoleOptions.Timing)
            {
                long elapsedTotal = 0;
                int times = consoleOptions.Time;
                while (times > 0)
                {
                    var s = Stopwatch.StartNew();
                    ax.Run(script, dir);
                    s.Stop();
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Elapsed MS: {0}", s.ElapsedMilliseconds);
                    Console.ForegroundColor = ConsoleColor.White;
                    elapsedTotal += s.ElapsedMilliseconds;
                    times--;
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Average Elapsed MS: {0}", elapsedTotal/consoleOptions.Time);
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadLine();
            }
            else
            {
                ax = new AxInterpreter(consoleOptions.Debug);
                ax.Run(script, dir);
            }
            //}
            //catch (Exception ex)
            //{
            //	Console.ForegroundColor = ConsoleColor.Red;
            //	Console.WriteLine("\n\n\nERROR RUNNING SCRIPT:\n\t{0}", ex.Message);
            //	Console.Write("Call History:\n\t");
            //	if(ax != null) ax.CallStack.ForEach(x => Console.Write("{0}, ", x));
            //	Console.WriteLine("[error]");
            //	Console.ReadLine();
            //}
        }
    }
}