using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Kuvalda.Tree;

namespace Kuvalda.Repository
{
    public class CommitService : ICommitService
    {
        public readonly ITreeCreator TreeCreator;
        public readonly IEntityObjectStorage<CommitModel> CommitStorage;
        public readonly IEntityObjectStorage<TreeNode> TreeStorage;
        public readonly IEntityObjectStorage<IDictionary<string, string>> HashStorage;
        public readonly IHashModificationFactory HashesFactory;
        public readonly IObjectStorage BlobStorage;
        public readonly IFileSystem FileSystem;

        public CommitService(IEntityObjectStorage<CommitModel> commitStorage,
            IEntityObjectStorage<TreeNode> treeStorage,
            IEntityObjectStorage<IDictionary<string, string>> hashStorage, ITreeCreator treeCreator,
            IHashModificationFactory hashesFactory, IObjectStorage blobStorage, IFileSystem fileSystem)
        {
            CommitStorage = commitStorage;
            TreeStorage = treeStorage;
            HashStorage = hashStorage;
            TreeCreator = treeCreator;
            HashesFactory = hashesFactory;
            BlobStorage = blobStorage;
            FileSystem = fileSystem;
        }


        public async Task<CommitDto> GetCommit(string chash)
        {
            if (CommitStorage.IsExists(chash))
            {
                return null;
            }

            var commit = await CommitStorage.Get(chash);

            if (string.IsNullOrEmpty(commit.HashesAddress))
            {
                throw new NullReferenceException(nameof(commit.HashesAddress));
            }
            
            var hashes = await HashStorage.Get(commit.HashesAddress);
            var tree = await TreeStorage.Get(commit.TreeHash);

            return new CommitDto()
            {
                Commit = commit,
                Hashes = hashes,
                Tree = tree
            };
        }

        public async Task<CommitDto> CreateCommit(string path, string prevChash)
        {
            var tree = await TreeCreator.Create(path);
            var prevCommit = await CommitStorage.Get(prevChash);

            var hashes = await HashesFactory.CreateHashes(await TreeStorage.Get(prevCommit.TreeHash), tree);

            var commitObject = new CommitModel()
            {
                Parents = new[] {prevChash},
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

        public async Task<string> StoreCommit(CommitDto commit)
        {
            var hhash = await HashStorage.Store(commit.Hashes);
            var thash = await TreeStorage.Store(commit.Tree);

            var waitStoreTasks = new List<Task>();
            
            foreach (var commitHash in commit.Hashes)
            {
                var filePath = FileSystem.Path.Combine(commit.Path, commitHash.Value);
                using (var file = FileSystem.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    waitStoreTasks.Add(Task.Run(() => BlobStorage.Set(commitHash.Key, file)));
                }
            }

            await Task.WhenAll(waitStoreTasks);

            commit.Commit.HashesAddress = hhash;
            commit.Commit.TreeHash = thash;
            
           return await CommitStorage.Store(commit.Commit);
        }
    }
}