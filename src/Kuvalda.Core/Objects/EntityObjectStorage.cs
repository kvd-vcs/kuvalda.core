using System;
using System.IO;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class EntityObjectStorage<TEntity> : IEntityObjectStorage<TEntity>
    {
        public readonly IObjectStorage Storage;
        public readonly ISerializationProvider SerializationProvider;
        public readonly IHashComputeProvider HashComputeProvider;

        public EntityObjectStorage(IObjectStorage storage, ISerializationProvider serializationProvider,
            IHashComputeProvider hashComputeProvider)
        {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            SerializationProvider =
                serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));
            HashComputeProvider = hashComputeProvider ?? throw new ArgumentNullException(nameof(hashComputeProvider));
        }

        public bool IsExists(string key)
        {
            return Storage.Exist(key);
        }

        public Task<TEntity> Get(string key)
        {
            return Task.Run(() => SerializationProvider.Deserialize<TEntity>(Storage.Get(key)));
        }

        public Task<string> Store(TEntity entity)
        {
            return Task.Run(async () =>
            {
                using (var stream = new MemoryStream())
                {
                    SerializationProvider.Serialize(entity, stream);
                    stream.Position = 0;
                    var hash = await HashComputeProvider.Compute(stream);
                    stream.Position = 0;
                    Storage.Set(hash, stream);
                    return hash;
                }
            });
        }
    }
}