using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IKeyValueStorage
    {
        /// <summary>
        ///     Get the value by key
        /// </summary>
        /// <returns> string if key exist, null if not exist </returns>
        Task<string> Get(string key);

        Task Set(string key, string value);
    }
}