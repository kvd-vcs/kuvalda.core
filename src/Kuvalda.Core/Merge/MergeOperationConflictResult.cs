namespace Kuvalda.Core.Merge
{
    public class MergeOperationConflictResult : MergeOperationResult
    {
        public string[] ConflictedFiles { get; set; }
    }
}