using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface ICommitGetService
    {
        Task<CommitDto> GetCommit(string chash);
    }
}