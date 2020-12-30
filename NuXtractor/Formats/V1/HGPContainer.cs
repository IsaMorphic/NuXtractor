using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    using Models;
    using Materials;
    using Textures;

    public class HGPContainer : Container, ITextureContainer
    {
        public HGPContainer(string path) : base("hgo_v1", path)
        {
        }

        protected override Task<Material> GetNewMaterialAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<Model> GetNewModelAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<Texture> GetNewTextureAsync(int id)
        {

            var textures = data.textures;

            var stream = textures.data[id];
            stream.Seek(0, SeekOrigin.Begin);

            var info = new DDSInfo(stream);
            await info.LoadAsync();

            var texture = new DDSTexture(id, info, stream);

            return texture;
        }
    }
}
