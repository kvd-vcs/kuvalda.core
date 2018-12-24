using System.Collections.Generic;

namespace Kuvalda.Core
{
    public interface IHashTableCreator
    {
        IDictionary<string, string> Compute(IEnumerable<FlatTreeItem> items, string context = "");
    }
}