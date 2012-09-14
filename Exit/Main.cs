using System;
using axScript3;

namespace Exit
{
    public class Main
    {
        [ExportAx("EXITCODE_ERROR", "If the type given by the exit point matches this, then an error was thrown.")] public const int EXIT_CODE_ERROR = 0;

        [ExportAx("EXITCODE_CLEAN", "If the type given by the exit point matches this, then the script finished execution as expected.")] public const int EXIT_CODE_CLEAN = 1;

        [HookAx("ExitPoint", "Similarly to EntryPoint, this label defines the function to call when code exits.")]
        public static void InitExitHandler(AxInterpreter ax, AxFunction func)
        {
            ax.ScriptError += exception => func.Call<Boolean>(ax, new object[] {EXIT_CODE_ERROR, exception});
            ax.ScriptEnd += exception => func.Call<Boolean>(ax, new object[] { EXIT_CODE_CLEAN, exception });
        }
    }
}