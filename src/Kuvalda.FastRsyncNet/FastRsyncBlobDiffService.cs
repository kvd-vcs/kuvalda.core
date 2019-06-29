using System;
using System.Threading.Tasks;
using Kuvalda.Core;
using Kuvalda.Core.Diff;

namespace Kuvalda.FastRsyncNet
{
    public class FastRsyncBlobDiffService : IBlobDiffService
    {
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly IDifferenceEntriesCreator _differenceEntries;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;

        public FastRsyncBlobDiffService(IEntityObjectStorage<CommitModel> commitStorage,
            IDifferenceEntriesCreator differenceEntries, IEntityObjectStorage<TreeNode> treeStorage)
        {
            _commitStorage = commitStorage ?? throw new ArgumentNullException(nameof(commitStorage));
            _differenceEntries = differenceEntries ?? throw new ArgumentNullException(nameof(differenceEntries));
            _treeStorage = treeStorage ?? throw new ArgumentNullException(nameof(treeStorage));
        }

        public Task<DiffResult> Compress(string srcCHash, string dstCHash)
        {
            /*
            var srcCommit = await _commitStorage.Get(srcCHash);
            var dstCommit = await _commitStorage.Get(dstCHash);

            var srcTree = await _treeStorage.Get(srcCommit.TreeHash);
            var dstTree = await _treeStorage.Get(dstCommit.TreeHash);

            var diff = _differenceEntries.Create(srcTree, dstTree);
            */
            throw new NotImplementedException();
        }
    }
}