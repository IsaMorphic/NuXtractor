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

namespace NuXtractor
{
    public abstract class Container : FormattedFile, IModelContainer, IMaterialContainer, ITextureContainer
    {
        protected Container(string formatName, string path) : base(formatName, path)
        {
        }

        public Dictionary<int, Model> CachedModels { get; private set; }
        public abstract int ModelCount { get; }

        public Dictionary<int, Material> CachedMaterials { get; private set; }
        public abstract int MaterialCount { get; }

        public Dictionary<int, Texture> CachedTextures { get; private set; }
        public abstract int TextureCount { get; }

        protected override Task OnLoadAsync()
        {
            CachedModels = new Dictionary<int, Model>();

            CachedMaterials = new Dictionary<int, Material>();

            CachedTextures = new Dictionary<int, Texture>();

            return base.OnLoadAsync();
        }

        public async Task<Model> GetModelAsync(int id)
        {
            if (!CachedModels.ContainsKey(id))
                CachedModels.Add(id, await GetNewModelAsync(id));
            return CachedModels[id];
        }
        protected abstract Task<Model> GetNewModelAsync(int id);

        public async Task<Material> GetMaterialAsync(int id)
        {
            if (!CachedMaterials.ContainsKey(id))
                CachedMaterials.Add(id, await GetNewMaterialAsync(id));
            return CachedMaterials[id];
        }
        protected abstract Task<Material> GetNewMaterialAsync(int id);

        public async Task<Texture> GetTextureAsync(int id)
        {
            if (!CachedTextures.ContainsKey(id))
                CachedTextures.Add(id, await GetNewTextureAsync(id));
            return CachedTextures[id];
        }
        protected abstract Task<Texture> GetNewTextureAsync(int id);
    }
}
