using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IKeyValueStorage
    {
        Task<string> Get(string key);
        Task Set(string key, string value);
    }
}