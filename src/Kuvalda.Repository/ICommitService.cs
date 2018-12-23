using System.Threading.Tasks;

namespace Kuvalda.Repository
{
    public interface ICommitService
    {
        Task<CommitDto> GetCommit(string chash);
        Task<CommitDto> CreateCommit(string path, string prevChash);
        Task<string> StoreCommit(CommitDto commit);
    }
}