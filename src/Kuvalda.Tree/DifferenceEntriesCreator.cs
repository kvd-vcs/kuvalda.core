using System;
using System.Linq;

namespace Kuvalda.Tree
{
    public class DifferenceEntriesCreator : IDifferenceEntriesCreator
    {
        private readonly IFlatTreeCreator _flatCreator;
        private readonly IFlatTreeDiffer _flatDiffer;

        public DifferenceEntriesCreator(IFlatTreeCreator flatCreator, IFlatTreeDiffer flatDiffer)
        {
            if (flatCreator == null)
            {
                throw new ArgumentNullException(nameof(flatCreator));
            }
            
            if (flatDiffer == null)
            {
                throw new ArgumentNullException(nameof(flatDiffer));
            }
            
            _flatCreator = flatCreator;
            _flatDiffer = flatDiffer;
        }
        
        public DifferenceEntries Create(TreeNode left, TreeNode right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }
            
            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }
            
            var flatTreeLeft = _flatCreator.Create(left);
            var flatTreeRight = _flatCreator.Create(right);

            var removed = _flatDiffer.Except(flatTreeLeft, flatTreeRight).Select(i => i.Name);
            var modified = _flatDiffer.Difference(flatTreeLeft, flatTreeRight).Select(i => i.Name);
            var added = _flatDiffer.Except(flatTreeRight, flatTreeLeft).Select(i => i.Name);
            
            return new DifferenceEntries(added, modified, removed);
        }
    }
}