namespace Kuvalda.Core.Merge
{
    public class MergeOperationSuccessResult : MergeOperationResult
    {
        public string LeftParent { get; set; }
        public string RightParent { get; set; }
        public string BaseCommit { get; set; }
        public TreeNode MergedTree { get; set; }
    }
}