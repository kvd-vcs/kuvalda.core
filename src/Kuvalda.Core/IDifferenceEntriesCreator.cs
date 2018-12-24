namespace Kuvalda.Core
{
    public interface IDifferenceEntriesCreator
    {
        DifferenceEntries Create(TreeNode left, TreeNode right);
    }
}