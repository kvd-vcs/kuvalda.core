using System.Collections.Generic;

namespace Kuvalda.Core
{
    public interface IFlatTreeDiffer
    {
        IEnumerable<FlatTreeItem> Except(IEnumerable<FlatTreeItem> left, IEnumerable<FlatTreeItem> right);
        IEnumerable<FlatTreeItem> Intersect(IEnumerable<FlatTreeItem> left, IEnumerable<FlatTreeItem> right);
        IEnumerable<FlatTreeItem> Difference(IEnumerable<FlatTreeItem> left, IEnumerable<FlatTreeItem> right);
    }
}