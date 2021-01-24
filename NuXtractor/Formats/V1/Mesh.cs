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

using NuXtractor.Materials;
using NuXtractor.Models;

using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    public class Mesh : Model
    {
        public Mesh Next { get; }

        private ElementStream ElemStream { get; }
        private VertexStream VtxStream { get; }

        private Vertex[] CachedVerticies { get; set; }
        private Triangle[] CachedTris { get; set; }

        public Mesh(Material material, Mesh next, ElementStream elemStream, VertexStream vtxStream) : base(material)
        {
            Next = next;
            ElemStream = elemStream;
            VtxStream = vtxStream;
        }

        public override async Task<Vertex[]> GetVerticesAsync()
        {
            if (CachedVerticies != null) return CachedVerticies;

            var xArr = await VtxStream.GetAttributeArray(VertexAttribute.X);
            var yArr = await VtxStream.GetAttributeArray(VertexAttribute.Y);
            var zArr = await VtxStream.GetAttributeArray(VertexAttribute.Z);

            var uArr = await VtxStream.GetAttributeArray(VertexAttribute.U);
            var vArr = await VtxStream.GetAttributeArray(VertexAttribute.V);

            var cArr = await VtxStream.GetAttributeArray(VertexAttribute.Color);

            var vtxArr = new Vertex[VtxStream.Count];

            for (int i = 0; i < vtxArr.Length; i++)
            {
                var x = BitConverter.Int32BitsToSingle(xArr[i]);
                var y = BitConverter.Int32BitsToSingle(yArr[i]);
                var z = BitConverter.Int32BitsToSingle(zArr[i]);

                var u = BitConverter.Int32BitsToSingle(uArr[i]);
                var v = BitConverter.Int32BitsToSingle(vArr[i]);

                var pos = new Vector3(x, y, z);
                var tex = new Vector2(u, v);

                var c = BitConverter.GetBytes(cArr[i]);

                var r = c[0] / 255.0f;
                var g = c[1] / 255.0f;
                var b = c[2] / 255.0f;

                var a = c[3] / 255.0f;

                var color = new Color(r, g, b, a);

                vtxArr[i] = new Vertex(pos, tex, color);
            }

            CachedVerticies = vtxArr;
            return vtxArr;
        }

        public Task<int[]> GetIndiciesAsync()
        {
            return ElemStream.ReadElementsAsync();
        }

        public override async Task<Triangle[]> GetTrianglesAsync()
        {
            if (CachedTris != null) return CachedTris;

            var idxArr = await GetIndiciesAsync();
            Triangle[] tris = new Triangle[idxArr.Length - 2];

            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] = new Triangle(idxArr[i + i % 2], idxArr[i - i % 2 + 1], idxArr[i + 2]);
            }

            CachedTris = tris;
            return tris;
        }

        public override async Task WriteToOBJAsync(StreamWriter writer, Matrix4x4? transform = null)
        {
            await base.WriteToOBJAsync(writer, transform);
            if (Next != null)
            {
                await Next.WriteToOBJAsync(writer, transform);
            }
        }
    }
}
