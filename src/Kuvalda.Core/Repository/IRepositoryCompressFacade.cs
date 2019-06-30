using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IRepositoryCompressFacade
    {
        /// <summary>
        ///     Create patch use delta compression between commits
        /// </summary>
        Task<CompressResult> Compress(CompressOptions options);
        
        /// <summary>
        ///     Apply exists patch
        /// </summary>
        Task<PatchResult> Patch(PatchOptions options);
    }
}