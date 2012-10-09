using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace axScript3
{
    public static class AxModuleLoader
    {
        internal static void Load(AxInterpreter i, string p)
        {
            if (!p.EndsWith(".dll"))
            {
                p = p + ".dll";
            }

            if (!i.Modules.Contains(p))
            {
                if (i.Debug)
                {
                    Console.WriteLine("Loading Module: \"{0}\"", p);
                }
                i.Modules.Add(p);
                var fp = Path.GetFullPath(p);
                var a = Assembly.LoadFile(fp);
                foreach (var t in a.GetTypes())
                {
                    foreach (var m in t.GetMethods())
                    {
                        foreach (var attr in m.GetCustomAttributes(true))
                        {
                            var axFunctionMarker = attr as ExportAx;
                            if (axFunctionMarker != null)
                            {
                                if (i.Debug)
                                {
                                    Console.Write("  Importing Function: '{0}'", axFunctionMarker.Name);
                                    Console.CursorLeft = 50;
                                    Console.WriteLine(axFunctionMarker.Description);
                                }
                                i.RegisterFunction(axFunctionMarker.Name, new NetFunction(m));
                            }
                            var axHookMarker = attr as HookAx;
                            if (axHookMarker != null)
                            {
                                if (i.Debug)
                                {
                                    Console.Write("  Importing Hook Tag: '{0}'", axHookMarker.Tag);
                                    Console.CursorLeft = 50;
                                    Console.WriteLine(axHookMarker.Description);
                                }
                                i.RegisterHook(axHookMarker.Tag, new NetFunction(m));
                            }
                        }
                    }

                    foreach (var prop in t.GetProperties())
                    {
                        foreach (var axFunctionMarker in prop.GetCustomAttributes(true).OfType<ExportAx>())
                        {
                            if (i.Debug)
                            {
                                Console.Write("  Importing Property [ReadOnly]: '{0}'", axFunctionMarker.Name);
                                Console.CursorLeft = 50;
                                Console.WriteLine(axFunctionMarker.Description);
                            }
                            i.RegisterFunction(axFunctionMarker.Name, new NetFunction(prop.GetGetMethod()));
                        }
                    }

                    foreach (var field in t.GetFields())
                    {
                        foreach (var axFunctionMarker in field.GetCustomAttributes(true).OfType<ExportAx>())
                        {
                            if (i.Debug)
                            {
                                Console.Write("  Importing Field: '{0}'", axFunctionMarker.Name);
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