using System;
using System.Collections;
using System.Text;

namespace axScript3
{
    public static class ExtensionClasses
    {
        public static void Print<T>(this T a)
        {
            Console.WriteLine(a.AdvToString());
        }

        public static int ToInt(this double d)
        {
            return Convert.ToInt32(d);
        }

        public static String AdvToString<T>(this T a)
        {
            var sb = new StringBuilder();
            if (a is IEnumerable && !(a is String))
            {
                sb.Append("{ ");
                foreach (var b in (IEnumerable) a)
                {
                    sb.Append(String.Format("{0} ", b.AdvToString()));
                }
                sb.Append("}");
            }
            else

            {
                sb.Append(a);
            }

            return sb.ToString();
        }
    }
}