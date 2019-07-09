namespace Kuvalda.Core.Merge
{
    public interface ITreeMergeService
    {
        TreeNode Merge(TreeNode left, TreeNode right);
    }

    public class TreeMergeService : ITreeMergeService
    {
        public TreeNode Merge(TreeNode left, TreeNode right)
        {
            return null;
        }
    }
}