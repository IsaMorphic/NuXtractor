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

using Rainbow.ImgLib.Common;
using Rainbow.ImgLib.Encoding;
using Rainbow.ImgLib.Encoding.Implementation;
using Rainbow.ImgLib.Filters;
using SkiaSharp;
using System;

namespace NuXtractor.Textures
{
    public class CTXTexture : Texture
    {
        private ushort Type { get; }

        private byte[] Palette { get; }

        public CTXTexture(int width, int height, byte[] palette, byte[] data, ushort type) : base(width, height, data)
        {
            Type = type;
            Palette = palette;
        }

        public override SKBitmap ToBitmap()
        {
            SKColor[] colors = null;
            if (Palette != null)
            {
                colors = ColorCodec
                    .CODEC_16BITBE_RGB5A3
                    .DecodeColors(Palette);
            }
            switch (Type)
            {
                case 0x08:
                    return new ImageDecoderIndexed(
                        Data, Width, Height,
                        IndexCodec.FromBitPerPixel(4, ByteOrder.BigEndian),
                        colors, new TileFilter(4, 8, 8, Width, Height)
                        ).DecodeImage();
                case 0x09:
                    return new ImageDecoderIndexed(
                        Data, Width, Height,
                        IndexCodec.FromBitPerPixel(8, ByteOrder.BigEndian),
                        colors, new TileFilter(8, 8, 4, Width, Height)
                        ).DecodeImage();
                case 0x0E:
                    return new ImageDecoderDirectColor(
                        Data, Width, Height,
                        new ColorCodecDXT1Gamecube(
                            Width, Height
                            )
                        ).DecodeImage();
                default:
                    throw new Exception("Unrecognized image format.");
            }
        }
    }
}
