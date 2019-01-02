using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface ICommitStoreService
    {
        Task<string> StoreCommit(CommitDto commit);
    }
}