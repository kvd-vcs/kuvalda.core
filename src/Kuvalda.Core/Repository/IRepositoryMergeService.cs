using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IRepositoryMergeService
    {
        Task<MergeResult> Merge(MergeOptions options);
    }
}