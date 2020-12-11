using MightyStruct;
using MightyStruct.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NuXtractor
{
    public class FormattedStream : IDisposable
    {
        private static Dictionary<string, IType> CachedTypes { get; }
        static FormattedStream()
        {
            CachedTypes = new Dictionary<string, IType>();
        }

        public string FormatName { get; }
        public Stream Stream { get; }

        public dynamic data { get; private set; }

        public FormattedStream(string formatName, Stream stream)
        {
            FormatName = formatName;
            Stream = stream;
        }

        public async Task LoadAsync()
        {
            IType format;
            if (CachedTypes.ContainsKey(FormatName))
            {
                format = CachedTypes[FormatName];
            }
            else
            {
                using (var formatStream = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Mighty\\{FormatName}.xml"))
                {
                    format = Parser.ParseFromStream(formatStream);
                    CachedTypes.Add(FormatName, format);
                }
            }

            data = await format.Resolve(new Context(Stream));
            await data.ParseAsync();

            await OnLoadAsync();
        }

        protected virtual Task OnLoadAsync()
        {
            return Task.CompletedTask;
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
