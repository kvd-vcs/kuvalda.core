using System.IO;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IObjectStorage
    {
        Task<bool> Exist(string key);
        Task<Stream> Get(string key);
        Task Set(string key, Stream obj);
    }
}