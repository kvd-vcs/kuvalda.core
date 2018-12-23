using System.Collections.Generic;

namespace Kuvalda.Tree
{
    public interface IHashTableCreator
    {
        IDictionary<string, string> Compute(IEnumerable<FlatTreeItem> items, string context = "");
    }
}