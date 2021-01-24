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
    public class UnknownTexture : Texture
    {
        public UnknownTexture(int id, int width, int height, int levels, Stream stream) : base(id, width, height, levels, stream)
        {
        }

        public override Task<Image<RgbaVector>> ReadImageAsync()
        {
            throw new NotImplementedException();
        }

        public override Task WriteImageAsync(Image<RgbaVector> image)
        {
            throw new NotImplementedException();
        }
    }
}
