using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class CommitGetService : ICommitGetService
    {
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly IEntityObjectStorage<IDictionary<string, string>> _hashStorage;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;

        public CommitGetService(IEntityObjectStorage<CommitModel> commitStorage,
            IEntityObjectStorage<IDictionary<string, string>> hashStorage, IEntityObjectStorage<TreeNode> treeStorage)
        {
            _commitStorage = commitStorage;
            _hashStorage = hashStorage;
            _treeStorage = treeStorage;
        }

        public async Task<CommitDto> GetCommit(string chash)
        {
            if (string.IsNullOrEmpty(chash))
            {
                throw new ArgumentNullException(nameof(chash));
            }

            var commit = await _commitStorage.Get(chash);
            
            var tree = await _treeStorage.Get(commit.TreeHash);

            return new CommitDto()
            {
                Commit = commit,
                Tree = tree
            };
        }
    }
}