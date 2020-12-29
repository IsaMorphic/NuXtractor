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

using SixLabors.ImageSharp.PixelFormats;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuXtractor.Textures.DXT
{
    public class DXT3Texture : DXT1Texture
    {
        private ISerializer<byte> Bytes { get; }

        public DXT3Texture(int id, int width, int height, int levels, Endianness endianness, Stream stream) : base(id, width, height, levels, endianness, stream)
        {
            Bytes = new UInt8Serializer();
        }

        private async Task<float[]> ReadAlphaAsync()
        {
            float[] alphas = new float[16];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j += 2)
                {
                    var pair = await Bytes.ReadFromStreamAsync(Stream);

                    var left = pair & 0x0F;
                    var right = (pair & 0xF0) >> 4;

                    alphas[i * 4 + 3 - (j + 0)] = left / 15.0f;
                    alphas[i * 4 + 3 - (j + 1)] = right / 15.0f;
                }
            }

            return alphas;
        }

        protected override async Task<RgbaVector[]> ReadTileAsync()
        {
            float[] alphas = await ReadAlphaAsync();
            RgbaVector[] tile = await base.ReadTileAsync();

            return tile
                .Select((c, i) => new RgbaVector(c.R, c.G, c.B, alphas[i]))
                .ToArray();
        }

        private async Task WriteAlphaAsync(float[] alphas)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j += 2)
                {
                    var left = (byte)Math.Clamp(alphas[i * 4 + 3 - (j + 0)] * 15.0f, 0.0f, 15.0f);
                    var right = (byte)Math.Clamp(alphas[i * 4 + 3 - (j + 1)] * 15.0f, 0.0f, 15.0f);

                    var pair = (byte)(left | (right << 4));
                    await Bytes.WriteToStreamAsync(Stream, pair);
                }
            }
        }

        protected override async Task WriteTileAsync(RgbaVector[] tile)
        {
            float[] alphas = tile.Select(c => c.A).ToArray();
            await WriteAlphaAsync(alphas);

            var noAlpha = tile
                .Select(c => new RgbaVector(c.R, c.G, c.B))
                .ToArray();
            await base.WriteTileAsync(noAlpha);
        }
    }
}
