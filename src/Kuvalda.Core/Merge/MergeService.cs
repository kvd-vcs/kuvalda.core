using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core.Merge
{
    public class MergeService : IMergeService
    {
        private readonly ICommitGetService _commitGetter;
        private readonly IBaseCommitFinder _commitFinder;
        private readonly IDifferenceEntriesCreator _entriesCreator;

        public MergeService(ICommitGetService commitGetter, IBaseCommitFinder commitFinder, IDifferenceEntriesCreator entriesCreator)
        {
            _commitGetter = commitGetter ?? throw new ArgumentNullException(nameof(commitGetter));
            _commitFinder = commitFinder ?? throw new ArgumentNullException(nameof(commitFinder));
            _entriesCreator = entriesCreator ?? throw new ArgumentNullException(nameof(entriesCreator));
        }

        public async Task<MergeOperationResult> Merge(string leftChash, string rightChash)
        {
            if (string.IsNullOrEmpty(leftChash))
            {
                throw new ArgumentNullException(nameof(leftChash));
            }
            
            if (string.IsNullOrEmpty(rightChash))
            {
                throw new ArgumentNullException(nameof(rightChash));
            }
            
            var baseCHash = await _commitFinder.FindBase(leftChash, rightChash);
            if (baseCHash == null)
            {
                return new MergeOperationInconsistentTreesResult();
            }

            var leftCommit = await _commitGetter.GetCommit(leftChash);
            var rightCommit = await _commitGetter.GetCommit(rightChash);
            var baseCommit = await _commitGetter.GetCommit(baseCHash);

            var leftDiff = _entriesCreator.Create(baseCommit.Tree, leftCommit.Tree);
            var rightDiff = _entriesCreator.Create(baseCommit.Tree, rightCommit.Tree);

            var modifiedConflict = leftDiff.Modified.Intersect(rightDiff.Modified).ToArray();

            if (modifiedConflict.Any())
            {
                return new MergeOperationConflictResult()
                {
                    ConflictedFiles = modifiedConflict
                };
            }
            
            return null;
        }
    }
}