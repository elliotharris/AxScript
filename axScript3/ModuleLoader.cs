using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace axScript3
{
    public static class AxModuleLoader
    {
        internal static void Load(AxInterpreter i, string p)
        {
            String fp = Path.GetFullPath(p.Substring(1,p.Length-2));
            Assembly a = Assembly.LoadFile(fp);
            foreach (var t in a.GetTypes())
            {
                if (!typeof(AxSharpModule).IsAssignableFrom(t)) continue;
                ((AxSharpModule)Activator.CreateInstance(t)).Load(i);
            }
        }
    }
}
