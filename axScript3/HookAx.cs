using System;

namespace axScript3
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HookAx : Attribute
    {
        public readonly string Tag;
        public readonly string Description;

        public HookAx(String tag, String description = "")
        {
            Tag = tag;
            Description = description;
        }
    }
}
