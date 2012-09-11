using System.IO;

namespace IO
{
    // ReSharper disable InconsistentNaming
    public class FileHandle
    {
        private readonly StreamReader _reader;
        private readonly FileStream _underlying;
        private readonly StreamWriter _writer;

        public FileHandle(FileStream fs)
        {
            _underlying = fs;
            if (fs.CanRead)
            {
                _reader = new StreamReader(fs);
            }
            if (fs.CanWrite)
            {
                _writer = new StreamWriter(fs);
            }
        }

        public void close()
        {
            _underlying.Dispose();
        }

        public char read()
        {
            int x = _reader.Read();
            if (x == -1)
            {
                return '\0';
            }
            return (char) x;
        }

        public string readline()
        {
            return _reader.ReadLine();
        }

        public void write(string f, params object[] pObjects)
        {
            _writer.Write(f, pObjects);
        }

        public void writeline(string f, params object[] pObjects)
        {
            _writer.WriteLine(f, pObjects);
        }
    }

    // ReSharper restore InconsistentNaming
}