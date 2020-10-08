/*
 *  Copyright 2020 Chosen Few Software
 *  This file is part of NuXtractor.
 *
 *  NuXtractor is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  NuXtractor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with NuXtractor.  If not, see <https://www.gnu.org/licenses/>.
 */

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Textures
{
    public abstract class Texture : IDisposable, IAsyncDisposable
    {
        public int Width { get; }
        public int Height { get; }

        protected Stream Stream { get; }

        public Texture(int width, int height, Stream stream)
        {
            Width = width;
            Height = height;

            Stream = stream;
        }

        public abstract Task<Image<RgbaVector>> ReadImageAsync();
        public abstract Task WriteImageAsync(Image<RgbaVector> pixels);

        public Task CopyToStreamAsync(Stream stream)
        {
            return Stream.CopyToAsync(stream);
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

        public ValueTask DisposeAsync()
        {
            return Stream.DisposeAsync();
        }
        #endregion
    }
}
