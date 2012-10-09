using axScript3;

namespace String
{
    public class Main
    {
        [ExportAx("string>format", "Creates a formatted string (uses String.Format)")]
        public static string Format(string f, params object[] o)
        {
            return string.Format(f, o);
        }

        [ExportAx("string>sub", "Substring function where negative lengths makes the length go from the end of the string.")]
        public static string Substring(string s, int start, int length = 0)
        {
            if (length < 0) length = s.Length + length - start + 1;

            return s.Substring(start, length);
        }

        [ExportAx("convert>string", "Converts the object to a string.")]
        public static string Convert(object e)
        {
            return e.ToString();
        }
    }
}
