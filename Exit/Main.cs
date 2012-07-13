using System;
using System.Collections.Generic;
using axScript3;

namespace Exit
{
    public class Main
    {
        [ExportAx("EXITCODE_ERROR", "If the type given by the exit point matches this, then an error was thrown.")] 
        public static int ExitCodeError = 1;

        [ExportAx("EXITCODE_CLEAN", "If the type given by the exit point matches this, then the script finished execution as expected.")]
        public static int ExitCodeClean = 1;

        [HookAx("ExitPoint", "Similarly to EntryPoint, this label defines the function to call when code exits.")]
        public static void InitExitHandler(AxInterpreter ax, AxFunction func)
        {
            ax.ScriptError += exception => func.Call<Boolean>(ax, new Dictionary<string, object>{ {"type", 1}, {"error", exception} });
        }
    }
}
