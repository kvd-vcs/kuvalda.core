using System;
using System.Collections.Generic;
using System.Linq;

namespace Kuvalda.Core.Merge
{
    public class ConflictDetectService : IConflictDetectService
    {
        private readonly IDifferenceEntriesCreator _entriesCreator;

        public ConflictDetectService(IDifferenceEntriesCreator entriesCreator)
        {
            _entriesCreator = entriesCreator ?? throw new ArgumentNullException(nameof(entriesCreator));
        }

        public IEnumerable<MergeConflict> Detect(TreeNode baseTree, TreeNode leftTree, TreeNode rightTree)
        {
            var leftDiff = _entriesCreator.Create(baseTree, leftTree);
            var rightDiff = _entriesCreator.Create(baseTree, rightTree);

            var modifiedConflict = leftDiff.Modified.Intersect(rightDiff.Modified);
            var addedRemovedConflict = leftDiff.Added.Intersect(rightDiff.Removed)
                .Union(leftDiff.Removed.Intersect(rightDiff.Added));
            var addedConflicts = leftDiff.Added.Intersect(rightDiff.Added);

            var conflicts = modifiedConflict.Union(addedRemovedConflict)
                .Union(addedConflicts)
                .ToArray();

            return null;
        }
    }
}