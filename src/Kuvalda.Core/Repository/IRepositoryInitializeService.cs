using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IRepositoryInitializeService
    {
        bool IsInitialized(string path);
        Task Initialize(string path);
    }
}