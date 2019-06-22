using System;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class TreeCreatorFiltered : ITreeCreator
    {
        public readonly ITreeCreator _TreeCreator;
        public readonly ITreeFilter _TreeFilter;

        public TreeCreatorFiltered(ITreeCreator treeCreator, ITreeFilter treeFilter)
        {
            _TreeCreator = treeCreator ?? throw new ArgumentNullException(nameof(treeCreator));
            _TreeFilter = treeFilter ?? throw new ArgumentNullException(nameof(treeFilter));
        }

        public async Task<TreeNode> Create(string path)
        {
            var original = await _TreeCreator.Create(path);
            var filtered = await _TreeFilter.Filter(original, path);
            return filtered;
        }
    }
}