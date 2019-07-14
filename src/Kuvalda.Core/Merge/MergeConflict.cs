namespace Kuvalda.Core.Merge
{
    public class MergeConflict
    {
        public string Path { get; set; }
        public MergeConflictReason LeftReason { get; set; }
        public MergeConflictReason RightReason { get; set; }

        public MergeConflict(string path, MergeConflictReason leftReason, MergeConflictReason rightReason)
        {
            Path = path;
            LeftReason = leftReason;
            RightReason = rightReason;
        }
    }
}