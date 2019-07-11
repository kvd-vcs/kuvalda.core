using System;
using System.Collections.Generic;
using System.Linq;
using Kuvalda.Core.Exceptions;

namespace Kuvalda.Core.Merge
{
    public class TreeMergeService : ITreeMergeService
    {
        public TreeNode Merge(TreeNode left, TreeNode right)
        {
            left = left ?? throw new ArgumentNullException(nameof(left));
            right = right ?? throw new ArgumentNullException(nameof(right));

            if (!left.Name.Equals(right.Name))
            {
                throw new ArgumentException("Mismatch nodes");
            }

            if (left is TreeNodeFolder leftFolder && right is TreeNodeFolder rightFolder)
            {
                return MergeFolders(leftFolder, rightFolder);
            }
            
            if (left is TreeNodeFile leftFile && right is TreeNodeFile rightFile)
            {
                return MergeFiles(leftFile, rightFile);
            }
            
            throw new ConflictTreeException();
        }

        private TreeNode MergeFolders(TreeNodeFolder leftFolder, TreeNodeFolder rightFolder)
        {
            var mergedChilds = MergeList(leftFolder.Nodes, rightFolder.Nodes);
            var result = new TreeNodeFolder(leftFolder.Name, mergedChilds);
            return result;
        }

        private TreeNode MergeFiles(TreeNodeFile leftFile, TreeNodeFile rightFile)
        {
            throw new NotImplementedException();
        }

        private TreeNode[] MergeList(IEnumerable<TreeNode> leftFolderNodes, IEnumerable<TreeNode> rightFolderNodes)
        {
            var mergeField = leftFolderNodes.Union(rightFolderNodes);
            return null;
        }
    }
}