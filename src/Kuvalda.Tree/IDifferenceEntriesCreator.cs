namespace Kuvalda.Tree
{
    public interface IDifferenceEntriesCreator
    {
        DifferenceEntries Create(TreeNode left, TreeNode right);
    }
}