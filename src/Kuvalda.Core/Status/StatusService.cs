using System.Threading.Tasks;

namespace Kuvalda.Core.Status
{
    public class StatusService : IStatusService
    {
        private readonly ITreeCreator _treeCreator;
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;
        private readonly IDifferenceEntriesCreator _differenceEntries; 

        public StatusService(ITreeCreator treeCreator, IEntityObjectStorage<CommitModel> commitStorage,
            IEntityObjectStorage<TreeNode> treeStorage, IDifferenceEntriesCreator differenceEntries)
        {
            _treeCreator = treeCreator;
            _commitStorage = commitStorage;
            _treeStorage = treeStorage;
            _differenceEntries = differenceEntries;
        }

        public async Task<DifferenceEntries> GetStatus(string path, string chash)
        {
            TreeNode tree = new TreeNodeFolder("");
            if (!string.IsNullOrEmpty(path))
            {
                tree = await _treeCreator.Create(path);
            }


            var targetCommit = await _commitStorage.Get(chash);
            var targetTree = await _treeStorage.Get(targetCommit.TreeHash);

            var diff = _differenceEntries.Create(tree, targetTree);

            return diff;
        }
    }
}