using MightyStruct;
using MightyStruct.Serializers;

using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuXtractor.Textures.DXT
{
    public class DXT5Texture : DXT1Texture
    {
        private ISerializer<byte> Bytes { get; }

        public DXT5Texture(int width, int height, int levels, Endianness endianness, Stream stream) : base(width, height, levels, endianness, stream)
        {
            Bytes = new UInt8Serializer();
        }

        private float Lerp(float v0, float v1, float t)
        {
            return v0 + t * (v1 - v0);
        }

        private byte ConvertAlphaToBits(float a)
        {
            return (byte)(Math.Clamp(a, 0.0f, 1.0f) * 255.0f);
        }

        private async Task<float[]> ReadAlphaAsync()
        {
            byte alpha0 = await Bytes.ReadFromStreamAsync(Stream);
            byte alpha1 = await Bytes.ReadFromStreamAsync(Stream);

            List<float> alphas = new List<float>
            {
                alpha0 / 255.0f,
                alpha1 / 255.0f
            };

            if (alpha0 > alpha1)
            {
                for (int i = 1; i < 7; i++)
                {
                    alphas.Add(Lerp(alphas[0], alphas[1], i / 7.0f));
                }
            }
            else
            {
                for (int i = 1; i < 5; i++)
                {
                    alphas.Add(Lerp(alphas[0], alphas[1], i / 5.0f));
                }

                alphas.Add(0.0f);
                alphas.Add(1.0f);
            }

            byte[] buffer = new byte[6];
            await Stream.ReadAsync(buffer, 0, buffer.Length);

            var bits = new BitArray(buffer)
                .Cast<bool>()
                .Select(b => b ? (byte)1 : (byte)0)
                .Reverse()
                .ToArray();

            int[] indexes = new int[16];
            for (int i = 0; i < buffer.Length * 8; i += 3)
            {
                int idx = i / 3;
                int y = idx / 4;
                int x = idx % 4;

                indexes[(3 - y) * 4 + x] = (bits[i + 0] << 2) | (bits[i + 1] << 1) | (bits[i + 2]);
            }

            return indexes
                .Select(i => alphas[i])
                .ToArray();
        }

        protected override async Task<RgbaVector[]> ReadTileAsync()
        {
            float[] alphas = await ReadAlphaAsync();
            RgbaVector[] tile = await base.ReadTileAsync();

            return tile
                .Select((c, i) => new RgbaVector(c.R, c.G, c.B, alphas[i]))
                .ToArray();
        }

        private float[] CalcAlphaPalette(RgbaVector[] tile)
        {
            var alphas = tile
                .Select(c => (float)Math.Floor(c.A * 255.0f + 0.5f) / 255.0f)
                .Where(a => a != 0.0f && a != 1.0f)
                .Distinct()
                .OrderBy(a => a)
                .ToArray();

            var result = alphas.Any() ? CalcBestExtremes(alphas) : (lo: 254.0f / 255.0f, hi: 1.0f);

            var lo = Math.Clamp(result.lo, 0.0f, 1.0f);
            var hi = Math.Clamp(result.hi, 0.0f, 1.0f);

            var alpha0 = ConvertAlphaToBits(lo);
            var alpha1 = ConvertAlphaToBits(hi);

            if (tile.Any(c => c.A == 0.0f || c.A == 1.0f))
            {
                if (alpha0 > alpha1)
                {
                    var temp = lo;
                    lo = hi;
                    hi = temp;
                }

                var palette = new List<float> { lo, hi };

                for (int i = 1; i < 5; i++)
                {
                    palette.Add(Lerp(palette[0], palette[1], i / 5.0f));
                }

                palette.Add(0.0f);
                palette.Add(1.0f);

                return palette.ToArray();
            }
            else
            {
                if (alpha0 < alpha1)
                {
                    var temp = lo;
                    lo = hi;
                    hi = temp;
                }
                else if (alpha0 == alpha1)
                {
                    hi = Math.Max(alpha1 - 1, 0) / 255.0f;
                }

                var palette = new List<float> { lo, hi };

                for (int i = 1; i < 7; i++)
                {
                    palette.Add(Lerp(palette[0], palette[1], i / 7.0f));
                }

                return palette.ToArray();
            }
        }

        private int[] CalcAlphaIndicies(float[] palette, RgbaVector[] tile)
        {
            return tile
                .Select(c =>
                   palette.Select(
                        (p, i) =>
                            (sc: Math.Abs(c.A - p), i)
                            )
                    .OrderBy(x => x.sc).First().i)
                .ToArray();
        }

        private async Task WriteAlphaAsync(RgbaVector[] tile)
        {
            var palette = CalcAlphaPalette(tile);
            var indices = CalcAlphaIndicies(palette, tile);

            var alpha0 = (byte)(palette[0] * 255.0f);
            var alpha1 = (byte)(palette[1] * 255.0f);

            await Bytes.WriteToStreamAsync(Stream, alpha0);
            await Bytes.WriteToStreamAsync(Stream, alpha1);

            var bits = new BitArray(new byte[6]);

            for (int i = 0; i < bits.Length; i += 3)
            {
                int j = i / 3;
                int y = j / 4;
                int x = j % 4;

                int idx = indices[(3 - y) * 4 + x];

                int bit0 = idx >> 2;
                int bit1 = (idx & 0x02) >> 1;
                int bit2 = (idx & 0x01);

                bits[bits.Length - (i + 1)] = bit0 == 1;
                bits[bits.Length - (i + 2)] = bit1 == 1;
                bits[bits.Length - (i + 3)] = bit2 == 1;
            }

            var buffer = new byte[6];
            bits.CopyTo(buffer, 0);

            await Stream.WriteAsync(buffer, 0, buffer.Length);
        }

        protected override async Task WriteTileAsync(RgbaVector[] tile)
        {
            await WriteAlphaAsync(tile);

            var noAlpha = tile
                .Select(c => new RgbaVector(c.R, c.G, c.B))
                .ToArray();

            await base.WriteTileAsync(noAlpha);
        }
    }
}
