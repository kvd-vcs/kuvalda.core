using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kuvalda.Core.Merge;

namespace Kuvalda.Core
{
    public class RepositoryMergeService : IRepositoryMergeService
    {
        private readonly IMergeService _mergeService;
        private readonly ICommitStoreService _storeService;

        public RepositoryMergeService(IMergeService mergeService, ICommitStoreService storeService)
        {
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        }

        public async Task<MergeResult> Merge(MergeOptions options)
        {
            var mergeResult = await _mergeService.Merge(options.LeftCHash, options.RightCHash);
            
            switch (mergeResult)
            {
                case MergeOperationSuccessResult success:
                    return await StoreCommit(success, options);
                
                case MergeOperationConflictResult conflicts:
                    return new MergeResultConflicts()
                    {
                        Conflicts = conflicts.ConflictedFiles
                    };
                
                case MergeOperationInconsistentTreesResult _:
                    return new MergeResultInconsistentTree();
                
                default:
                    throw new Exception("Unknown merge result");
            }
        }

        private async Task<MergeResult> StoreCommit(MergeOperationSuccessResult mergeResult, MergeOptions options)
        {
            var commitDto = new CommitDto()
            {
                Commit = new CommitModel()
                {
                    Labels = options.Labels ?? new Dictionary<string, string>(),
                    Parents = new List<string>()
                    {
                        mergeResult.LeftParent, mergeResult.RightParent
                    }
                },
                Tree = mergeResult.MergedTree,
                ItemsForWrite = new string[0]
            };
            var hash = await _storeService.StoreCommit(commitDto);

            return new MergeResultSuccess()
            {
                MergeCommitHash = hash
            };
        }
    }
}