using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core.Merge
{
    public class MergeService : IMergeService
    {
        private readonly ICommitGetService _commitGetter;
        private readonly IBaseCommitFinder _commitFinder;
        private readonly ITreeMergeService _mergeService;
        private readonly IConflictDetectService _detectService;

        public MergeService(ICommitGetService commitGetter, IBaseCommitFinder commitFinder, 
            ITreeMergeService mergeService, IConflictDetectService detectService)
        {
            _commitGetter = commitGetter ?? throw new ArgumentNullException(nameof(commitGetter));
            _commitFinder = commitFinder ?? throw new ArgumentNullException(nameof(commitFinder));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _detectService = detectService ?? throw new ArgumentNullException(nameof(detectService));
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

            var conflicts = _detectService.Detect(baseCommit.Tree, leftCommit.Tree, rightCommit.Tree).ToList();

            if (conflicts.Any())
            {
                return new MergeOperationConflictResult()
                {
                    ConflictedFiles = conflicts
                };
            }

            return new MergeOperationSuccessResult()
            {
                BaseCommit = baseCHash,
                LeftParent = leftChash,
                RightParent = rightChash,
                MergedTree = _mergeService.Merge(leftCommit.Tree, rightCommit.Tree)
            };
        }
    }
}