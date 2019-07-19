using System.Collections.Generic;

namespace Kuvalda.Core.Merge
{
    public interface IConflictDetectService
    {
        IEnumerable<MergeConflict> Detect(TreeNode baseTree, TreeNode leftTree, TreeNode rightTree);
    }
}