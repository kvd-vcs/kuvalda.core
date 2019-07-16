using System;
using System.Collections.Generic;
using System.Linq;

namespace Kuvalda.Core.Merge
{
    public class ConflictDetectService : IConflictDetectService
    {
        private readonly IDifferenceEntriesCreator _entriesCreator;
        private readonly IFlatTreeCreator _flatTreeCreator;

        public ConflictDetectService(IDifferenceEntriesCreator entriesCreator, IFlatTreeCreator flatTreeCreator)
        {
            _entriesCreator = entriesCreator ?? throw new ArgumentNullException(nameof(entriesCreator));
            _flatTreeCreator = flatTreeCreator ?? throw new ArgumentNullException(nameof(flatTreeCreator));
        }

        public IEnumerable<MergeConflict> Detect(TreeNode baseTree, TreeNode leftTree, TreeNode rightTree)
        {
            var leftDiff = _entriesCreator.Create(baseTree, leftTree);
            var rightDiff = _entriesCreator.Create(baseTree, rightTree);

            var modifiedConflict = leftDiff.Modified.Intersect(rightDiff.Modified);
            var addedRemovedLeft = leftDiff.Added.Intersect(rightDiff.Removed);
            var addedRemovedRight = leftDiff.Removed.Intersect(rightDiff.Added);
            var bothAdded = leftDiff.Added.Intersect(rightDiff.Added);

            if (bothAdded.Any() || modifiedConflict.Any())
            {
                var leftFlat = _flatTreeCreator.Create(leftTree).ToDictionary(i => i.Name, i => i.Node);
                var rightFlat = _flatTreeCreator.Create(rightTree).ToDictionary(i => i.Name, i => i.Node);

                bothAdded = bothAdded.Where(i => IsHashConflict(i, leftFlat, rightFlat));
                modifiedConflict = modifiedConflict.Where(i => IsHashConflict(i, leftFlat, rightFlat));
            }

            return modifiedConflict
                .Select(i => new MergeConflict(i, MergeConflictReason.Modify, MergeConflictReason.Modify))
                .Union(addedRemovedLeft.Select(i =>
                    new MergeConflict(i, MergeConflictReason.Added, MergeConflictReason.Removed)))
                .Union(addedRemovedRight.Select(i =>
                    new MergeConflict(i, MergeConflictReason.Removed, MergeConflictReason.Added)))
                .Union(bothAdded.Select(i =>
                    new MergeConflict(i, MergeConflictReason.Added, MergeConflictReason.Added)));
        }

        private bool IsHashConflict(string s, IDictionary<string, TreeNode> leftFlat, IDictionary<string, TreeNode> rightFlat)
        {
            var left = leftFlat[s];
            var right = rightFlat[s];

            if (left.GetType() != right.GetType())
            {
                return true;
            }

            if (left is TreeNodeFile leftFile && right is TreeNodeFile rightFile)
            {
                return !string.Equals(leftFile.Hash, rightFile.Hash);
            }

            return false;
        }
    }
}