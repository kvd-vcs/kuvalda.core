using System.IO;

namespace Kuvalda.Core
{
    public interface ISerializationProvider
    {
        void Serialize(object entity, Stream stream);
        T Deserialize<T>(Stream stream);
    }
}