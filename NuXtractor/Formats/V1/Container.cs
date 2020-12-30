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
using NuXtractor.Textures;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    public abstract class Container : FormattedFile, IModelContainer, IMaterialContainer, ITextureContainer
    {
        protected Container(string formatName, string path) : base(formatName, path)
        {
        }

        public Model[] CachedModels { get; private set; }
        public int ModelCount => data.models.list.desc.size;

        public Dictionary<long, Mesh> CachedSubModels { get; private set; }

        public Material[] CachedMaterials { get; private set; }
        public int MaterialCount => data.materials.desc.size;

        public Texture[] CachedTextures { get; private set; }
        public int TextureCount => data.textures.desc.size;

        protected override Task OnLoadAsync()
        {
            CachedModels = new Model[ModelCount];
            CachedSubModels = new Dictionary<long, Mesh>();

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
