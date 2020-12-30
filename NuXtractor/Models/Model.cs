using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NuXtractor.Models
{
    using Materials;

    public abstract class Model
    {
        public Model Next { get; }
        public Material Material { get; }

        public List<Model> SubModels
        {
            get
            {
                var subModels = new List<Model> { this };
                if (Next != null)
                    subModels.AddRange(Next.SubModels);
                return subModels;
            }
        }

        public Model(Model next, Material material)
        {
            Next = next;
            Material = material;
        }

        public abstract Task<UVCoord[]> GetUVsAsync();

        public abstract Task<Vertex[]> GetVerticesAsync();
        public abstract Task<int[]> GetIndiciesAsync();

        public abstract Task<Face[]> GetFacesAsync();

        public async Task WriteToOBJAsync(StreamWriter writer, Matrix4x4? transform = null)
        {
            await writer.WriteLineAsync($"usemtl material_{Material.Id}");

            var uvs = await GetUVsAsync();
            var verticies = await GetVerticesAsync();
            var faces = await GetFacesAsync();

            foreach (var v in verticies)
            {
                var pos = Vector3.Transform(v.XYZ, transform ?? Matrix4x4.Identity);
                await writer.WriteLineAsync($"v {pos.X} {pos.Y} {pos.Z} {v.Color}");
            }

            foreach (var uv in uvs)
            {
                await writer.WriteLineAsync($"vt {uv.U} {uv.V}");
            }

            foreach (var f in faces)
            {
                await writer.WriteAsync("f");
                foreach (var v in f.Vertices)
                {
                    await writer.WriteAsync($" {-(verticies.Length - v.Id)}/{-(uvs.Length - v.UV.Id)}");
                }
                await writer.WriteAsync("\n");
            }

            if (Next != null) await Next.WriteToOBJAsync(writer, transform ?? Matrix4x4.Identity);
        }
    }
}
