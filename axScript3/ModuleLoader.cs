﻿using System;
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
                            var axFunctionMarker = attr as ExportAx;
                            if (axFunctionMarker == null) continue;
                            if (i.Debug)
                            {
                                Console.Write("Importing Function: '{0}'", axFunctionMarker.Name);
                                Console.CursorLeft = 50;
                                Console.WriteLine(axFunctionMarker.Description);
                            }
                            i.RegisterFunction(axFunctionMarker.Name, new NetFunction(m));
                        }
                    }

                    foreach (var prop in t.GetProperties())
                    {
                        foreach (var attr in prop.GetCustomAttributes(true))
                        {
                            var axFunctionMarker = attr as ExportAx;
                            if (axFunctionMarker == null) continue;
                            if (i.Debug)
                            {
                                Console.Write("Importing Property [ReadOnly]: '{0}'", axFunctionMarker.Name);
                                Console.CursorLeft = 50;
                                Console.WriteLine(axFunctionMarker.Description);
                            }
                            i.RegisterFunction(axFunctionMarker.Name, new NetFunction(prop.GetGetMethod()));
                        }
                    }

                    foreach (var field in t.GetFields())
                    {
                        foreach (var attr in field.GetCustomAttributes(true))
                        {
                            var axFunctionMarker = attr as ExportAx;
                            if (axFunctionMarker == null) continue;
                            if (i.Debug)
                            {
                                Console.Write("Importing Field: '{0}'", axFunctionMarker.Name);
                                Console.CursorLeft = 50;
                                Console.WriteLine(axFunctionMarker.Description);
                            }
                            i.Variables.Add(axFunctionMarker.Name, field.GetValue(null));
                        }
                    }
                }
            }
        }
    }
}