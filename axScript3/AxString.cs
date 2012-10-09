namespace axScript3
{
    public class AxString
    {
        private readonly string _inner;
        public readonly int Length;

        public AxString(string b)
        {
            _inner = b;
            Length = _inner.Length;
        }

        public AxString(string f, params object[] o)
        {
            _inner = string.Format(f, o);
        }

        public static implicit operator string(AxString x)
        {
            return x._inner;
        }

        public static implicit operator AxString(string x)
        {
            return new AxString(x);
        }

        public AxString Substring(int start, int length = 0)
        {
            if (length < 0) length = _inner.Length + length - start + 1;
            return _inner.Substring(start, length);
        }

        public AxString ToLower()
        {
            return _inner.ToLower();
        }

        public AxString ToUpper()
        {
            return _inner.ToUpper();
        }

        public AxString Replace(string x, string y)
        {
            return _inner.Replace(x, y);
        }

        public AxString Replace(char x, char y)
        {
            return _inner.Replace(x, y);
        }

        public int IndexOf(char c, int l = 0, int x = 0)
        {
            return x == 0 ? _inner.IndexOf(c, l) : _inner.IndexOf(c, l, x);
        }

        public AxString Trim(params char[] c)
        {
            return c.Length == 0 ? _inner.Trim() : _inner.Trim(c);
        }

        public AxString TrimStart(params char[] c)
        {
            return c.Length == 0 ? _inner.TrimStart() : _inner.TrimStart(c);
        }

        public AxString TrimEnd(params char[] c)
        {
            return c.Length == 0 ? _inner.TrimEnd() : _inner.TrimEnd(c);
        }

        public bool StartsWith(string x)
        {
            return _inner.StartsWith(x);
        }

        public bool StartsWith(char x)
        {
            return _inner[0] == x;
        }

        public static AxString Format(string f, params object[] o)
        {
            return new AxString(string.Format(f, o));
        }
    }
}
