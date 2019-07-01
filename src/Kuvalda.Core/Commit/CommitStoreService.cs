using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class CommitStoreService : ICommitStoreService
    {
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;
        private readonly IFileSystem _fileSystem;
        private readonly IObjectStorage _blobStorage;
        private readonly IFlatTreeCreator _flatTreeCreator;

        public CommitStoreService(IEntityObjectStorage<CommitModel> commitStorage,
            IEntityObjectStorage<TreeNode> treeStorage,
            IFileSystem fileSystem, IObjectStorage blobStorage, IFlatTreeCreator flatTreeCreator)
        {
            _commitStorage = commitStorage;
            _treeStorage = treeStorage;
            _fileSystem = fileSystem;
            _blobStorage = blobStorage;
            _flatTreeCreator = flatTreeCreator;
        }

        public async Task<string> StoreCommit(CommitDto commit)
        {
            var thash = await _treeStorage.Store(commit.Tree);

            var waitStoreTasks = new List<Task>();
            var flatTree = _flatTreeCreator.Create(commit.Tree).ToDictionary(c => c.Name, c => c.Node);
            
            foreach (var path in commit.ItemsForWrite)
            {
                var fileNode = flatTree[path] as TreeNodeFile;

                if (fileNode == null)
                {
                    continue;
                }
                
                waitStoreTasks.Add(Task.Run(async () =>
                {
                    var filePath = _fileSystem.Path.Combine(commit.Path, path);
                    using (var file = _fileSystem.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await _blobStorage.Set(fileNode.Hash, file);
                    }
                }));
            }

            await Task.WhenAll(waitStoreTasks);
            
            commit.Commit.TreeHash = thash;

            return await _commitStorage.Store(commit.Commit);
        }
    }
}