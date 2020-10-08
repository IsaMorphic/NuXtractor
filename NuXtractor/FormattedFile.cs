using MightyStruct.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor
{
    public class FormattedFile : IDisposable
    {
        private string FormatName { get; }
        private string Path { get; }

        private Stream Stream { get; set; }

        protected dynamic data { get; private set; }

        public FormattedFile(string formatName, string path)
        {
            FormatName = formatName;
            Path = path;
        }

        public async Task LoadAsync()
        {
            using (var formatStream = File.OpenRead($".\\Mighty\\{FormatName}.xml"))
            {
                var format = Parser.ParseFromStream(formatStream);

                Stream = File.Open(Path, FileMode.Open, FileAccess.ReadWrite);
                data = await format.Resolve(new Context(Stream));
                await data.ParseAsync();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stream.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
