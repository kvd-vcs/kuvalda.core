using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IChangesCompressService
    {
        Task<CompressModel> Compress(string srcCHash, string dstCHash);
    }
}