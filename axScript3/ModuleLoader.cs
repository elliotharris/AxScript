using System;
using System.IO;
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
                i.Modules.Add(p);
                String fp = Path.GetFullPath(p);
                Assembly a = Assembly.LoadFile(fp);
                foreach (Type t in a.GetTypes())
                {
                    foreach (MethodInfo m in t.GetMethods())
                    {
                        foreach (object attr in m.GetCustomAttributes(true))
                        {
                            var axFunctionMarker = attr as ExportAx;
                            if (axFunctionMarker != null)
                            {
                                if (i.Debug)
                                {
                                    Console.Write("Importing Function: '{0}'", axFunctionMarker.Name);
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
                                    Console.Write("Importing Hook Tag: '{0}'", axHookMarker.Tag);
                                    Console.CursorLeft = 50;
                                    Console.WriteLine(axHookMarker.Description);
                                }
                                i.RegisterHook(axHookMarker.Tag, new NetFunction(m));
                            }
                        }
                    }

                    foreach (PropertyInfo prop in t.GetProperties())
                    {
                        foreach (object attr in prop.GetCustomAttributes(true))
                        {
                            var axFunctionMarker = attr as ExportAx;
                            if (axFunctionMarker == null)
                            {
                                continue;
                            }
                            if (i.Debug)
                            {
                                Console.Write("Importing Property [ReadOnly]: '{0}'", axFunctionMarker.Name);
                                Console.CursorLeft = 50;
                                Console.WriteLine(axFunctionMarker.Description);
                            }
                            i.RegisterFunction(axFunctionMarker.Name, new NetFunction(prop.GetGetMethod()));
                        }
                    }

                    foreach (FieldInfo field in t.GetFields())
                    {
                        foreach (object attr in field.GetCustomAttributes(true))
                        {
                            var axFunctionMarker = attr as ExportAx;
                            if (axFunctionMarker == null)
                            {
                                continue;
                            }
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