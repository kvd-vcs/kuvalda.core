using System;
using System.Threading.Tasks;

namespace Kuvalda.Core.Merge
{
    public class MergeService : IMergeService
    {
        private readonly ICommitGetService _commitGetter;
        private readonly IBaseCommitFinder _commitFinder;

        public MergeService(ICommitGetService commitGetter, IBaseCommitFinder commitFinder)
        {
            _commitGetter = commitGetter ?? throw new ArgumentNullException(nameof(commitGetter));
            _commitFinder = commitFinder ?? throw new ArgumentNullException(nameof(commitFinder));
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
            
            var baseCommit = await _commitFinder.FindBase(leftChash, rightChash);

            if (baseCommit == null)
            {
                return new MergeOperationInconsistentTreesResult();
            }

            return null;
        }
    }
}