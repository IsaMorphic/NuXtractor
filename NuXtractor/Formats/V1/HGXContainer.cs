using MightyStruct.Serializers;

using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    using Models;
    using Materials;
    using Textures;
    using Textures.DXT;

    public class HGXContainer : Container, ITextureContainer
    {
        public HGXContainer(string path) : base("hgx_v1", path)
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

        protected override Task<Texture> GetNewTextureAsync(int id)
        {
            var textures = data.textures;

            var width = (int)textures.desc[id].width;
            var height = (int)textures.desc[id].height;

            var levels = (int)textures.desc[id].levels;
            var type = (int)textures.desc[id].type;

            var stream = textures.data[id];

            Texture texture;
            switch (type)
            {
                case 0x0C:
                    texture = new DXT1Texture(id, width, height, levels, Endianness.LittleEndian, stream);
                    break;
                case 0x0F:
                    texture = new DXT5Texture(id, width, height, levels, Endianness.LittleEndian, stream);
                    break;
                default:
                    texture = new UnknownTexture(id, width, height, levels, stream);
                    break;
            }
            return Task.FromResult(texture);
        }
    }
}
