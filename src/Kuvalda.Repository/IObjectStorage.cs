using System.IO;

namespace Kuvalda.Repository
{
    public interface IObjectStorage
    {
        bool Exist(string key);
        Stream Get(string key);
        void Set(string key, Stream obj);
    }
}