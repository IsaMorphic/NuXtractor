using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Textures
{
    public class UnknownTexture : ITexture
    {
        public int Width { get; }
        public int Height { get; }

        public Stream Stream { get; }

        public UnknownTexture(int width, int height, Stream stream)
        {
            Width = width;
            Height = height;

            Stream = stream;
        }

        public Task<Image<RgbaVector>> ReadImageAsync()
        {
            throw new NotImplementedException();
        }

        public Task WriteImageAsync(Image<RgbaVector> image)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            return Stream.DisposeAsync();
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
