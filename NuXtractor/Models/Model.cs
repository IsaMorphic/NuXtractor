using NuXtractor.Materials;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.Models
{
    public abstract class Model
    {
        public int Id { get; }
        public Material Material { get; }

        public Model(int id, Material material)
        {
            Id = id;
            Material = material;
        }

        public abstract Task<UVCoord[]> GetUVsAsync();

        public abstract Task<Vertex[]> GetVerticesAsync();
        public abstract Task<int[]> GetIndiciesAsync();

        public abstract Task<Face[]> GetFacesAsync();

        public async Task WriteToOBJAsync(StreamWriter writer, Matrix4x4? transform = null)
        {
            await writer.WriteLineAsync($"usemtl material_{Material.Id}");
            await writer.WriteLineAsync($"g model_{Id}");

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
        }
    }
}
