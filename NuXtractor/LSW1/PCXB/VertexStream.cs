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

using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.LSW1.PCXB
{
    public enum VertexAttribute
    {
        X = 0,
        Y = 4,
        Z = 8,
        U = 28,
        V = 32,
        Color = 24
    }

    public class VertexStream
    {
        private const uint STRIDE = 32;

        private static ISerializer<int> DWords { get; } =
            new SInt32Serializer(Endianness.LittleEndian);

        private Stream Stream { get; }

        public int Count { get; }

        public VertexStream(Stream stream)
        {
            Stream = stream;
            Count = (int)(Stream.Length / (STRIDE + 4));
        }

        public async Task<int[]> GetAttributeArray(VertexAttribute attr)
        {
            var attrs = new int[Count];

            Stream.Seek((long)attr, SeekOrigin.Begin);
            for (int i = 0; i < attrs.Length; i++)
            {
                attrs[i] = await DWords.ReadFromStreamAsync(Stream);
                Stream.Seek(STRIDE, SeekOrigin.Current);
            }

            return attrs;
        }
    }
}
