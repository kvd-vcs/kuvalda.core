using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;

namespace Kuvalda.Core
{
    public class EntityObjectStorage<TEntity> : IEntityObjectStorage<TEntity>
    {
        private readonly IObjectStorage _storage;
        private readonly ISerializationProvider _serializationProvider;
        private readonly IHashComputeProvider _hashComputeProvider;
        private readonly ILogger _logger;

        public EntityObjectStorage(IObjectStorage storage, ISerializationProvider serializationProvider,
            IHashComputeProvider hashComputeProvider, ILogger logger)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _serializationProvider =
                serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));
            _hashComputeProvider = hashComputeProvider ?? throw new ArgumentNullException(nameof(hashComputeProvider));
            _logger = logger;
        }

        public async Task<bool> IsExists(string key)
        {
            return await _storage.Exist(key);
        }

        public async Task<TEntity> Get(string key)
        {
            using (var stream = await _storage.Get(key))
            {
                _logger?.Debug("Requested object {key} with type {type}", key, typeof(TEntity));
                return _serializationProvider.Deserialize<TEntity>(stream);
            }
        }

        public async Task<string> Store(TEntity entity)
        {
            using (var stream = new MemoryStream())
            {
                _serializationProvider.Serialize(entity, stream);
                stream.Position = 0;
                var hash = await _hashComputeProvider.Compute(stream);
                stream.Position = 0;
                await _storage.Set(hash, stream);
                _logger?.Debug("Stored object {key} with type {type}", hash, typeof(TEntity));
                return hash;
            }
        }
    }
}