using System.Collections.Generic;

namespace Kuvalda.Tree
{
    public interface IFlatTreeCreator
    {
        IEnumerable<FlatTreeItem> Create(TreeNode tree, string context = "/");
    }
}