using System;

namespace axScript3
{
    [AttributeUsage(AttributeTargets.All)]
    public class ExportAx : Attribute
    {
        public readonly string Description;
        public readonly string Name;

        public ExportAx(String name, String description = "")
        {
            Name = name;
            Description = description;
        }
    }
}