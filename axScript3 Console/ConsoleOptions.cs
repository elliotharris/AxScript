using CommandLine;

namespace axScript3Console
{
    internal class ConsoleOptions
    {
        [Option("d", "debug", Required = false, HelpText = "Runs script in debugging mode")] public bool Debug = false;
        [Option("s", "script", Required = false, HelpText = "Script file to run. Defaults to \"main.ax3\"")] public string Script = "main.ax3";
        [Option("x", "times", Required = false, HelpText = "How many times to run the script, for timing checks")] public int Time = 1;
        [Option("t", "time", Required = false, HelpText = "Runs script and uses a stopwatch to time. Set -x to say how many times to time the script.")] public bool Timing = false;
    }
}