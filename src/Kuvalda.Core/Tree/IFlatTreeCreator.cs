using System.Collections.Generic;

namespace Kuvalda.Core
{
    public interface IFlatTreeCreator
    {
        IEnumerable<FlatTreeItem> Create(TreeNode tree, string context = "/");
    }
}