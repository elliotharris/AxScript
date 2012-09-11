using System;
using System.IO;
using axScript3;

namespace IO
{
    public class IOStuff
    {
        [ExportAx("__IOVERSION", "Version of the IO Library")] public static int IOVersion = 201;

        [ExportAx("fileopen", "Opens up a file for reading or writing")]
        public static FileHandle FileOpen(string fileName, string mode)
        {
            FileMode fm;
            FileAccess fa;
            switch (mode)
            {
                case "r":
                    fm = FileMode.Open;
                    fa = FileAccess.Read;
                    break;
                case "w":
                    fm = FileMode.Create;
                    fa = FileAccess.Write;
                    break;
                case "a":
                    fm = FileMode.Append;
                    fa = FileAccess.Write;
                    break;
                case "rw":
                    fm = FileMode.OpenOrCreate;
                    fa = FileAccess.ReadWrite;
                    break;
                default:
                    throw new Exception("Invalid file mode specified");
            }
            return new FileHandle(File.Open(fileName, fm, fa));
        }
    }
}