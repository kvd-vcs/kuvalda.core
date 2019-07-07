using System.Threading.Tasks;

namespace Kuvalda.Core.Merge
{
    public interface IMergeService
    {
        Task<MergeOperationResult> Merge(string leftChash, string rightChash);
    }
}