using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class CommitCreateService : ICommitCreateService
    {
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly ITreeCreator _treeCreator;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;
        private readonly IHashModificationFactory _hashesFactory;

        public CommitCreateService(IEntityObjectStorage<CommitModel> commitStorage, ITreeCreator treeCreator,
            IEntityObjectStorage<TreeNode> treeStorage, IHashModificationFactory hashesFactory)
        {
            _commitStorage = commitStorage;
            _treeCreator = treeCreator;
            _treeStorage = treeStorage;
            _hashesFactory = hashesFactory;
        }

        public async Task<CommitDto> CreateCommit(string path, string prevChash = null)
        {
            var tree = await _treeCreator.Create(path);
            TreeNode prevTree = new TreeNodeFolder("");
            var parentChashes = new string[0];

            if (!string.IsNullOrEmpty(prevChash))
            {
                var prevCommit = await _commitStorage.Get(prevChash);
                prevTree = await _treeStorage.Get(prevCommit.TreeHash);
                parentChashes = new[] {prevChash};
            }

            var hashes = await _hashesFactory.CreateHashes(prevTree, tree);

            var commitObject = new CommitModel()
            {
                Parents = parentChashes,
                Labels = new Dictionary<string, string>(),
                HashesAddress = null,
                TreeHash = null,
            };

            return new CommitDto()
            {
                Path = path,
                Tree = tree,
                Commit = commitObject,
                Hashes = hashes
            };
        }
    }
}