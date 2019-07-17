namespace Kuvalda.Core.Merge
{
    public class MergeOperationSuccessResult : MergeOperationResult
    {
        public string LeftParent { get; set; }
        public string RightParent { get; set; }
        public string BaseCommit { get; set; }
        public TreeNode MergedTree { get; set; }
        
        protected bool Equals(MergeOperationSuccessResult other)
        {
            return string.Equals(LeftParent, other.LeftParent) && string.Equals(RightParent, other.RightParent) &&
                   string.Equals(BaseCommit, other.BaseCommit) && Equals(MergedTree, other.MergedTree);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MergeOperationSuccessResult) obj);
        }
    }
}