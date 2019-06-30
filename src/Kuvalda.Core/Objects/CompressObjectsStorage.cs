using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Kuvalda.Core
{
    public interface ICompressObjectsStorage
    {
        Task<CompressModel> Get(string srcCommit, string dstCommit);
        Task<string> Set(CompressModel model);
    }

    public class CompressObjectsStorage : ICompressObjectsStorage
    {
        private readonly IEntityObjectStorage<CompressModel> _storage;
        private readonly IKeyValueStorage _kvStorage;
        private readonly IHashComputeProvider _hashCompute;
        private readonly ILogger _logger;

        public CompressObjectsStorage(IEntityObjectStorage<CompressModel> storage, IKeyValueStorage kvStorage,
            ILogger logger, IHashComputeProvider hashCompute)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _kvStorage = kvStorage ?? throw new ArgumentNullException(nameof(kvStorage));
            _hashCompute = hashCompute ?? throw new ArgumentNullException(nameof(hashCompute));
            _logger = logger;
        }

        public async Task<CompressModel> Get(string srcCommit, string dstCommit)
        {
            var hash = await GetHash($"{srcCommit}-{dstCommit}");

            var indexItem = await _kvStorage.Get(hash);
            
            if(string.IsNullOrEmpty(indexItem))
            {
                _logger.Debug("Patch from {srcCommit} to {dstCommit} not found", srcCommit, dstCommit);
                return null;
            }

            if (!_storage.IsExists(indexItem))
            {
                _logger.Warning("DB consistency warning. Not found patch from index reference. src commit: {srcCommit}, dst commit: {dstCommit}",
                    srcCommit, dstCommit);
                return null;
            }

            return await _storage.Get(indexItem);
        }

        public async Task<string> Set(CompressModel model)
        {
            var hash = await _storage.Store(model);
            var indexHash = await GetHash($"{model.Source}-{model.Destination}");
            await _kvStorage.Set(indexHash, hash);
            return hash;
        }

        private async Task<string> GetHash(string data)
        {
            var hashSource = Encoding.UTF8.GetBytes(data);
            using (var stream = new MemoryStream(hashSource))
            {
                return await _hashCompute.Compute(stream);
            }
        }
    }
}