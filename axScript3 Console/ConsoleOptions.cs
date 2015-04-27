using System.ComponentModel;
using CommandLine;

namespace axScript3Console
{
    internal class ConsoleOptions
    {
        [Option('d', "debug", DefaultValue = false, Required = false, HelpText = "Runs script in debugging mode")]
        public bool Debug { get; set; }

        [Option('s', "script", DefaultValue = "main.ax3", Required = false, HelpText = "Script file to run. Defaults to \"main.ax3\"")] 
        public string Script { get; set; }

        [Option('x', "times", DefaultValue = 1, Required = false, HelpText = "How many times to run the script, for timing checks")] 
        public int Time { get; set; }

        [Option('t', "time", DefaultValue = false, Required = false, HelpText = "Runs script and uses a stopwatch to time. Set -x to say how many times to time the script.")] 
        public bool Timing { get; set; }
    }
}