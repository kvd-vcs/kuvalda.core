using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class CommitStoreService : ICommitStoreService
    {
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;
        private readonly IEntityObjectStorage<IDictionary<string, string>> _hashStorage;
        private readonly IFileSystem _fileSystem;
        private readonly IObjectStorage _blobStorage;

        public CommitStoreService(IEntityObjectStorage<CommitModel> commitStorage,
            IEntityObjectStorage<TreeNode> treeStorage,
            IEntityObjectStorage<IDictionary<string, string>> hashStorage,
            IFileSystem fileSystem, IObjectStorage blobStorage)
        {
            _commitStorage = commitStorage;
            _treeStorage = treeStorage;
            _hashStorage = hashStorage;
            _fileSystem = fileSystem;
            _blobStorage = blobStorage;
        }

        public async Task<string> StoreCommit(CommitDto commit)
        {
            var hhash = await _hashStorage.Store(commit.Hashes);
            var thash = await _treeStorage.Store(commit.Tree);

            var waitStoreTasks = new List<Task>();
            
            foreach (var commitHash in commit.Hashes)
            {
                var filePath = _fileSystem.Path.Combine(commit.Path, commitHash.Key);
                using (var file = _fileSystem.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    waitStoreTasks.Add(Task.Run(() => _blobStorage.Set(commitHash.Value, file)));
                }
            }

            await Task.WhenAll(waitStoreTasks);

            commit.Commit.HashesAddress = hhash;
            commit.Commit.TreeHash = thash;

            return await _commitStorage.Store(commit.Commit);
        }
    }
}