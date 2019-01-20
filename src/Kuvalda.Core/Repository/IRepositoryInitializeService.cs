using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IRepositoryInitializeService
    {
        bool IsInitialized();
        Task Initialize();
    }
}