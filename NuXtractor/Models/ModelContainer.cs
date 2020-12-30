using System.Threading.Tasks;

namespace NuXtractor.Models
{
    public interface IModelContainer
    {
        public int ModelCount { get; }
        Task<Model> GetModelAsync(int id);
    }
}
