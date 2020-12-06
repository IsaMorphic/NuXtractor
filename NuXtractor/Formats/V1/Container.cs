using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    using Models;
    using Materials;
    using Textures;

    public abstract class Container : FormattedFile, IModelContainer, IMaterialContainer, ITextureContainer
    {
        protected Container(string formatName, string path) : base(formatName, path)
        {
        }

        public Model[] CachedModels { get; private set; }
        public int ModelCount => data.models.list.desc.size;

        public Material[] CachedMaterials { get; private set; }
        public int MaterialCount => data.materials.desc.size;

        public Texture[] CachedTextures { get; private set; }
        public int TextureCount => data.textures.desc.size;

        protected override Task OnLoadAsync()
        {
            CachedModels = new Model[ModelCount];
            CachedMaterials = new Material[MaterialCount];
            CachedTextures = new Texture[TextureCount];

            return base.OnLoadAsync();
        }

        public async Task<Model> GetModelAsync(int id)
        {
            if (CachedModels[id] == null)
                return CachedModels[id] = await GetNewModelAsync(id);
            else
                return CachedModels[id];
        }
        protected abstract Task<Model> GetNewModelAsync(int id);

        public async Task<Material> GetMaterialAsync(int id)
        {
            if (CachedMaterials[id] == null)
                return CachedMaterials[id] = await GetNewMaterialAsync(id);
            else
                return CachedMaterials[id];
        }
        protected abstract Task<Material> GetNewMaterialAsync(int id);

        public async Task<Texture> GetTextureAsync(int id)
        {
            if (CachedTextures[id] == null)
                return CachedTextures[id] = await GetNewTextureAsync(id);
            else
                return CachedTextures[id];
        }
        protected abstract Task<Texture> GetNewTextureAsync(int id);
    }
}
