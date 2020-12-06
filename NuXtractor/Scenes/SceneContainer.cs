using System.Threading.Tasks;

namespace NuXtractor.Scenes
{
    public interface ISceneContainer
    {
        Task<Scene> GetSceneAsync();
    }
}
