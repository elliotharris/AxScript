using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace axScript3
{
    [AttributeUsage(AttributeTargets.All)]
    public class ExportAx : Attribute
    {
        public readonly string Name;
        public readonly string Description;

        public ExportAx(String name, String description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
