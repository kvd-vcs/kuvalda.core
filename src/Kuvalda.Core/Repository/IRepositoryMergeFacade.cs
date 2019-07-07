using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IRepositoryMergeFacade
    {
        Task<MergeResult> Merge(MergeOptions options);
    }

    public class MergeOptions
    {
        public string[] Commits { get; set; }
    }

    public class MergeResult
    {
        public string MergeCommit { get; set; }
    }
}