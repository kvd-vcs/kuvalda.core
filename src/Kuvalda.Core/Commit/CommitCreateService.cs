using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class CommitCreateService : ICommitCreateService
    {
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly ITreeCreator _treeCreator;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;
        private readonly IHashModificationFactory _hashesFactory;
        private readonly IFlatTreeCreator _flatTreeCreator;

        public CommitCreateService(IEntityObjectStorage<CommitModel> commitStorage, ITreeCreator treeCreator,
            IEntityObjectStorage<TreeNode> treeStorage, IHashModificationFactory hashesFactory, IFlatTreeCreator flatTreeCreator)
        {
            _commitStorage = commitStorage;
            _treeCreator = treeCreator;
            _treeStorage = treeStorage;
            _hashesFactory = hashesFactory;
            _flatTreeCreator = flatTreeCreator;
        }

        public async Task<CommitDto> CreateCommit(string path = null, string prevChash = null)
        {
            TreeNode tree = new TreeNodeFolder("");
            if (!string.IsNullOrEmpty(path))
            {
                tree = await _treeCreator.Create(path);
            }

            TreeNode prevTree = new TreeNodeFolder("");
            var parentChashes = new string[0];

            if (!string.IsNullOrEmpty(prevChash))
            {
                var prevCommit = await _commitStorage.Get(prevChash);
                prevTree = await _treeStorage.Get(prevCommit.TreeHash);
                parentChashes = new[] {prevChash};
            }

            var hashes = await _hashesFactory.CreateHashes(prevTree, tree, path);

            FillNodesHashes(tree, prevTree, hashes);

            var commitObject = new CommitModel()
            {
                Parents = parentChashes,
                Labels = new Dictionary<string, string>(),
                TreeHash = null,
            };

            var itemsForWrite = hashes.Keys;

            return new CommitDto()
            {
                Path = path,
                Tree = tree,
                Commit = commitObject,
                ItemsForWrite = itemsForWrite
            };
        }

        private void FillNodesHashes(TreeNode tree, TreeNode prevTree, IDictionary<string, string> hashes)
        {
            var currentTreeFlat = _flatTreeCreator.Create(tree).ToList();
            var currentTreeTable = currentTreeFlat.ToDictionary(c => c.Name, c => c.Node);
            var prevTreeFlat = _flatTreeCreator.Create(prevTree).ToList();

            // Fill exists file hashes
            foreach (var prevTreeFlatItem in prevTreeFlat.Where(t => t.Node is TreeNodeFile))
            {
                if (currentTreeTable.TryGetValue(prevTreeFlatItem.Name, out var currentItem) &&
                    currentItem is TreeNodeFile fileNode)
                {
                    fileNode.Hash = ((TreeNodeFile) prevTreeFlatItem.Node).Hash;
                }
            }

            foreach (var hashItem in hashes)
            {
                if (currentTreeTable.TryGetValue(hashItem.Key, out var currentItem) && currentItem is TreeNodeFile fileNode)
                {
                    fileNode.Hash = hashItem.Value;
                }
            }
        }
    }
}