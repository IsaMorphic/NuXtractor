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

using MightyStruct;
using MightyStruct.Serializers;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Textures
{
    public abstract class Texture
    {
        private static ISerializer<ulong> Longs { get; } = new UInt64Serializer(EndianInfo.SystemEndianness);
        private static ISerializer<uint> Ints { get; } = new UInt32Serializer(EndianInfo.SystemEndianness);

        public int Id { get; }

        public int Width { get; }
        public int Height { get; }

        public int Levels { get; }

        protected Stream Stream { get; }

        public Texture(int id, int width, int height, int levels, Stream stream)
        {
            Id = id;

            Width = width;
            Height = height;

            Levels = levels;

            Stream = stream;
        }

        public abstract Task<Image<RgbaVector>> ReadImageAsync();
        public abstract Task WriteImageAsync(Image<RgbaVector> pixels);

        public async Task WritePatchAsync(Stream stream)
        {
            await Longs.WriteToStreamAsync(stream, (ulong)(Stream as SubStream).AbsoluteOffset);
            await Ints.WriteToStreamAsync(stream, (uint)Stream.Length);

            Stream.Seek(0, SeekOrigin.Begin);
            await Stream.CopyToAsync(stream);
        }

        public Task CopyToStreamAsync(Stream stream)
        {
            Stream.Seek(0, SeekOrigin.Begin);
            return Stream.CopyToAsync(stream);
        }

        public Task CopyFromStreamAsync(Stream stream)
        {
            Stream.Seek(0, SeekOrigin.Begin);
            return stream.CopyToAsync(Stream);
        }
    }
}
