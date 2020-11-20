using System.IO;

namespace NuXtractor
{
    public class FormattedFile : FormattedStream
    {
        public FormattedFile(string formatName, string path) : base(formatName, File.Open(path, FileMode.Open, FileAccess.ReadWrite))
        {
        }
    }
}
