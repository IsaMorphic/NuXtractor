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
    public class ElementStream
    {
        private static ISerializer<ushort> Words { get; } =
            new UInt16Serializer(Endianness.LittleEndian);

        private Stream Stream { get; }

        public int Count { get; }

        public ElementStream(Stream stream)
        {
            Stream = stream;
            Count = (int)(stream.Length / 2);
        }

        public async Task<int[]> ReadElementsAsync()
        {
            var elements = new int[Count];
            Stream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = await Words.ReadFromStreamAsync(Stream);
            }

            return elements;
        }
    }
}
