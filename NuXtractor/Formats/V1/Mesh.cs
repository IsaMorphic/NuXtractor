using System;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    using Materials;
    using Models;

    public class Mesh : Model
    {
        private int[] Indicies { get; }
        private VertexStream Stream { get; }

        private UVCoord[] CachedUVs { get; set; }
        private Vertex[] CachedVerticies { get; set; }
        private Face[] CachedFaces { get; set; }

        public Mesh(Model next, Material material, int[] indicies, VertexStream stream) : base(next, material)
        {
            Indicies = indicies;
            Stream = stream;
        }

        public override async Task<UVCoord[]> GetUVsAsync()
        {
            if (CachedUVs != null) return CachedUVs;

            var uArr = await Stream.GetAttributeArray(VertexAttribute.U);
            var vArr = await Stream.GetAttributeArray(VertexAttribute.V);

            var uvArr = new UVCoord[Stream.Length];

            for (int i = 0; i < uvArr.Length; i++)
            {
                var u = BitConverter.Int32BitsToSingle(uArr[i]);
                var v = BitConverter.Int32BitsToSingle(vArr[i]);

                uvArr[i] = new UVCoord(i, u, v);
            }

            CachedUVs = uvArr;

            return uvArr;
        }

        public override async Task<Vertex[]> GetVerticesAsync()
        {
            if (CachedVerticies != null) return CachedVerticies;

            var xArr = await Stream.GetAttributeArray(VertexAttribute.X);
            var yArr = await Stream.GetAttributeArray(VertexAttribute.Y);
            var zArr = await Stream.GetAttributeArray(VertexAttribute.Z);

            var cArr = await Stream.GetAttributeArray(VertexAttribute.Color);

            var uvArr = await GetUVsAsync();

            var vtxArr = new Vertex[Stream.Length];

            for (int i = 0; i < vtxArr.Length; i++)
            {
                var x = BitConverter.Int32BitsToSingle(xArr[i]);
                var y = BitConverter.Int32BitsToSingle(yArr[i]);
                var z = BitConverter.Int32BitsToSingle(zArr[i]);

                var xyz = new Vector3(x, y, z);
                var uv = uvArr[i];

                var c = BitConverter.GetBytes(cArr[i]);

                var r = c[0] / 255.0f;
                var g = c[1] / 255.0f;
                var b = c[2] / 255.0f;

                var a = c[3] / 255.0f;

                var color = new Color(r, g, b, a);

                vtxArr[i] = new Vertex(i, xyz, uv, color);
            }

            CachedVerticies = vtxArr;

            return vtxArr;
        }

        public override Task<int[]> GetIndiciesAsync()
        {
            return Task.FromResult(Indicies);
        }

        public override async Task<Face[]> GetFacesAsync()
        {
            if (CachedFaces != null) return CachedFaces;

            var vtxArr = await GetVerticesAsync();
            var idxArr = await GetIndiciesAsync();

            Face[] faceArr = new Face[idxArr.Length - 2];

            for (int i = 0; i < faceArr.Length; i++)
            {
                faceArr[i] = new Face(i, vtxArr[idxArr[i + 0]], vtxArr[idxArr[i + 1]], vtxArr[idxArr[i + 2]]);
            }

            CachedFaces = faceArr;

            return faceArr;
        }
    }
}
