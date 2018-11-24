using System;
using System.Collections.Generic;
using System.Linq;

namespace Kuvalda.Tree
{
    public class FlatTreeCreator : IFlatTreeCreator
    {
        public IEnumerable<FlatTreeItem> Create(TreeNode tree, string context = "")
        {
            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }

            var nameWithContext = (context + tree.Name).TrimStart('/');
            var treeItem = new FlatTreeItem(nameWithContext.Replace("//", "/"), tree);
            var currentItemList = new[] {treeItem};
            
            if (tree is TreeNodeFolder folder)
            {
                return currentItemList.Union(folder.Nodes.SelectMany(node =>
                    Create(node, nameWithContext + "/")));
            }

            return currentItemList;
        }
    }
}