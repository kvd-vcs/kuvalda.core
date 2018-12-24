using System;
using System.Collections.Generic;
using System.Linq;

namespace Kuvalda.Core
{
    public class FlatTreeDiffer : IFlatTreeDiffer
    {
        class FlatTreeSameTypeAndName : EqualityComparer<FlatTreeItem>
        {
            public override bool Equals(FlatTreeItem x, FlatTreeItem y)
                => x.Name == y.Name && x.GetType() == y.GetType();

            public override int GetHashCode(FlatTreeItem obj)
                => obj.Name.GetHashCode();
        }

        public IEnumerable<FlatTreeItem> Except(IEnumerable<FlatTreeItem> left, IEnumerable<FlatTreeItem> right)
            => left.Except(right, new FlatTreeSameTypeAndName());
        
        public IEnumerable<FlatTreeItem> Intersect(IEnumerable<FlatTreeItem> left, IEnumerable<FlatTreeItem> right)
            => left.Intersect(right, new FlatTreeSameTypeAndName());
        
        public IEnumerable<FlatTreeItem> Difference(IEnumerable<FlatTreeItem> left, IEnumerable<FlatTreeItem> right)
        {
            var leftIntersection = Intersect(left, right).OrderBy(i => i.Name).ToList();
            var rightIntersection = Intersect(right, left).OrderBy(i => i.Name).ToList();

            return rightIntersection.Where((item, i) => !item.Node.Equals(leftIntersection[i].Node));
        }
    }
}