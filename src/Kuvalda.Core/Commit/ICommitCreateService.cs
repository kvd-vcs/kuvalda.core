using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface ICommitCreateService
    {
        Task<CommitDto> CreateCommit(string path, string prevChash);
    }
}