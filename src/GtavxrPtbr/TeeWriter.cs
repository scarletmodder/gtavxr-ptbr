using System.IO;
using System.Text;

namespace GtavxrPtbr
{
    /// <summary>Writes the same text to the console and to the log file.</summary>
    internal sealed class TeeWriter : TextWriter
    {
        readonly TextWriter first, second;

        public TeeWriter(TextWriter first, TextWriter second)
        {
            this.first = first;
            this.second = second;
        }

        public override Encoding Encoding => first.Encoding;

        public override void Write(char value)
        {
            first.Write(value);
            second.Write(value);
        }

        public override void Write(string value)
        {
            first.Write(value);
            second.Write(value);
        }

        public override void WriteLine(string value)
        {
            first.WriteLine(value);
            second.WriteLine(value);
        }

        public override void Flush()
        {
            first.Flush();
            second.Flush();
        }
    }
}
