using System;
using System.Collections.Generic;
using System.Linq;
using Kuvalda.Core.Exceptions;

namespace Kuvalda.Core.Merge
{
    public class TreeMergeService : ITreeMergeService
    {
        private readonly INowDateTimeService _nowDateTimeService;

        public TreeMergeService(INowDateTimeService nowDateTimeService)
        {
            _nowDateTimeService = nowDateTimeService ?? throw new ArgumentNullException(nameof(nowDateTimeService));
        }

        public TreeNode Merge(TreeNode left, TreeNode right)
        {
            left = left ?? throw new ArgumentNullException(nameof(left));
            right = right ?? throw new ArgumentNullException(nameof(right));

            if (!left.Name.Equals(right.Name))
            {
                throw new ConflictTreeException($"Mismatch node names. left: {left}, right: {right}");
            }

            if (left is TreeNodeFolder leftFolder && right is TreeNodeFolder rightFolder)
            {
                return MergeFolders(leftFolder, rightFolder);
            }
            
            if (left is TreeNodeFile leftFile && right is TreeNodeFile rightFile)
            {
                return MergeFiles(leftFile, rightFile);
            }
            
            throw new ConflictTreeException($"Mismatch node types. left: {left.GetType()}, right: {right.GetType()}");
        }

        private TreeNode MergeFolders(TreeNodeFolder leftFolder, TreeNodeFolder rightFolder)
        {
            var mergedChilds = MergeList(leftFolder.Nodes, rightFolder.Nodes);
            var result = new TreeNodeFolder(leftFolder.Name, mergedChilds);
            return result;
        }

        private TreeNode MergeFiles(TreeNodeFile leftFile, TreeNodeFile rightFile)
        {
            if (leftFile.DeepEquals(rightFile))
            {
                return leftFile;
            }

            if (string.Equals(leftFile.Hash, rightFile.Hash))
            {
                var newNode = (TreeNodeFile)leftFile.Clone();
                newNode.ModificationTime = _nowDateTimeService.GetNow();
                return newNode;
            }
            
            throw new ConflictTreeException($"Conflict when merge files. Left: {{{leftFile}}}, right: {{{rightFile}}}");
        }

        private TreeNode[] MergeList(IEnumerable<TreeNode> leftFolderNodes, IEnumerable<TreeNode> rightFolderNodes)
        {
            var mergeField = leftFolderNodes.Union(rightFolderNodes);
            var result = mergeField.GroupBy(n => n.Name).Select(MergeUnionGroup);
            return result.ToArray();
        }

        private TreeNode MergeUnionGroup(IGrouping<string, TreeNode> group)
        {
            if (group.Count() > 2)
            {
                throw new ConflictTreeException("Cant merge group of 3 or more nodes");
            }

            if (group.Count() == 1)
            {
                return group.Single();
            }

            return Merge(group.First(), group.Last());
        }
    }
}