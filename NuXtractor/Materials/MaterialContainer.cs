using System.Threading.Tasks;

namespace NuXtractor.Materials
{
    public interface IMaterialContainer
    {
        int MaterialCount { get; }
        Task<Material> GetMaterialAsync(int id);
    }
}
