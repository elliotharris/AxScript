using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace axScript3
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExportAsAxFunction : Attribute
    {
        public readonly string Name;

        public ExportAsAxFunction(String name)
        {
            Name = name;
        }
    }
}
