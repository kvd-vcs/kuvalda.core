using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IHashTableCreator
    {
        Task<IDictionary<string, string>> Compute(IEnumerable<FlatTreeItem> items, string context = "");
    }
}