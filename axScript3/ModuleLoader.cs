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
            if (!i.Modules.Contains(p))
            {
                i.Modules.Add(p);
                String fp = Path.GetFullPath(p.Substring(1, p.Length - 2));
                Assembly a = Assembly.LoadFile(fp);
                foreach (var t in a.GetTypes())
                {
                    foreach (var m in t.GetMethods())
                    {
                        foreach (var attr in m.GetCustomAttributes(true))
                        {
                            var axFunctionMarker = attr as ExportAsAxFunction;
                            if (axFunctionMarker != null)
                            {
                                i.RegisterFunction((axFunctionMarker).Name, new NetFunction(m));
                            }
                        }
                    }
                }
            }
        }
    }
}
