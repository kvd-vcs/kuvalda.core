namespace Kuvalda.Core.Merge
{
    public interface ITreeMergeService
    {
        TreeNode Merge(TreeNode left, TreeNode right);
    }
}