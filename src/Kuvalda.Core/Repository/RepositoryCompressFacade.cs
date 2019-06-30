using System;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class RepositoryCompressFacade : IRepositoryCompressFacade
    {
        private readonly IChangesCompressService _compressService;
        private readonly IChangesDecompressService _decompressService;
        private readonly IEntityObjectStorage<CompressModel> _compressStorage;

        public RepositoryCompressFacade(IChangesCompressService compressService, IChangesDecompressService decompressService,
            IEntityObjectStorage<CompressModel> compressStorage)
        {
            _compressService = compressService ?? throw new ArgumentNullException(nameof(compressService));
            _decompressService = decompressService ?? throw new ArgumentNullException(nameof(decompressService));
            _compressStorage = compressStorage ?? throw new ArgumentNullException(nameof(compressStorage));
        }

        public async Task<CompressResult> Compress(CompressOptions options)
        {
            var compressModel =
                await _compressService.Compress(options.SourceCommitHash, options.DestinationCommitHash);
            var hash = await _compressStorage.Store(compressModel);
            
            return new CompressResult
            {
                Method = compressModel.Method,
                PatchHash = hash,
                CompressedFiles = compressModel.Deltas.Keys
            };
        }

        public async Task<PatchResult> Patch(PatchOptions options)
        {
            var patch = await _compressStorage.Get(options.PatchHash);
            await _decompressService.Apply(patch, options.RepositoryPath);
            return new PatchResult
            {
                Method = patch.Method,
                PatchedFiles = patch.Deltas.Keys
            };
        }
    }
}