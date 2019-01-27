using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface ICommitCreateService
    {
        Task<CommitDto> CreateCommit(string path = null, string prevChash = null); // TODO: implement multi-parent commit creation
    }
}