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

using SkiaSharp;

namespace NuXtractor.Textures
{
    public class IndexedTexture : Texture
    {
        public SKColor[] Colors { get; }

        public IndexedTexture(int width, int height, SKColor[] colors, byte[] data) : base(width, height, data)
        {
            Colors = colors;
        }

        public override SKBitmap ToBitmap()
        {
            SKBitmap bitmap = new SKBitmap(Width, Height);
            switch (Colors.Length)
            {
                case 16:
                    for (int x = 0; x < Width / 2; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            byte pair = Data[y * Width / 2 + x];
                            int index1 = pair & 15;
                            int index2 = pair >> 4;
                            bitmap.SetPixel(x * 2, y, Colors[index1]);
                            bitmap.SetPixel(x * 2 + 1, y, Colors[index2]);
                        }
                    }
                    break;
                case 256:
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            int index = Data[y * Width + x];
                            bitmap.SetPixel(x, y, Colors[index]);
                        }
                    }
                    break;
            }
            return bitmap;
        }
    }
}
