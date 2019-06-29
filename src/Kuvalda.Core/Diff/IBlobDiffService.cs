using System.Threading.Tasks;

namespace Kuvalda.Core.Diff
{
    public interface IBlobDiffService
    {
        Task<DiffResult> Compress(string srcCHash, string dstCHash);
    }
}