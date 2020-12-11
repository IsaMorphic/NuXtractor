using System.IO;

namespace NuXtractor
{
    public class FormattedFile : FormattedStream
    {
        public string Path { get; }

        public FormattedFile(string formatName, string path) : base(formatName, File.Open(path, FileMode.Open, FileAccess.ReadWrite))
        {
            Path = path;
        }
    }
}
