using System;
using System.Diagnostics;
using CommandLine;
using axScript3;
using System.IO;

namespace axScript3Console
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			AxInterpreter ax = null;
			//try
			//{
				Console.WriteLine("AxScript 3 Console");
				Console.WriteLine("^^^^^^^^ ^ ^^^^^^^");                   

				var consoleOptions = new ConsoleOptions();
				var parser = new CommandLineParser();
				if (!parser.ParseArguments(args, consoleOptions))
				{
					throw new Exception("Invalid options specified");
				}

				//Console.WriteLine(consoleOptions.Script);

				var script = File.ReadAllText(consoleOptions.Script);
		        var dir = Path.GetDirectoryName(consoleOptions.Script);
                ax = new AxInterpreter(consoleOptions.Debug);
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
