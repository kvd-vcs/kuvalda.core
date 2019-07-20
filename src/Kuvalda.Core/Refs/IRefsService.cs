using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IRefsService
    {
        Task<bool> Exists(string name);
        Task<Reference> Get(string name);
        Task<string> GetCommit(string name);
        Task Store(string name, Reference reference);
        Task<Reference> GetHead();
        Task<string> GetHeadCommit();
        Task SetHead(Reference value);
        Task<string[]> GetAll();
    }
}
