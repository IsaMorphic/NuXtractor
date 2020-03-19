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

using Rainbow.ImgLib.Encoding;
using Rainbow.ImgLib.Filters;
using SkiaSharp;
using System;

namespace NuXtractor.Textures
{
    public class PNTTexture : Texture
    {
        public int BitDepth { get; }
        public SKColor[] Colors { get; }

        public byte[] Pixels { get; }

        public PNTTexture(int width, int height, byte[] data, SKColor[] colors, byte[] pixels) : base(width, height, data)
        {
            Colors = colors;
            BitDepth = (int)Math.Log2(Colors.Length);

            Pixels = pixels;
        }

        public override SKBitmap ToBitmap()
        {
            return new ImageDecoderIndexed(
                Pixels, Width, Height, 
                IndexCodec.FromBitPerPixel(BitDepth), Colors, 
                null, new TIM2PaletteFilter(BitDepth)
                ).DecodeImage();
        }
    }
}
