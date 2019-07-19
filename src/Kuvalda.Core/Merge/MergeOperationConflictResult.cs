using System.Collections.Generic;
using System.Linq;

namespace Kuvalda.Core.Merge
{
    public class MergeOperationConflictResult : MergeOperationResult
    {
        public IEnumerable<MergeConflict> ConflictedFiles { get; set; }
        
        protected bool Equals(MergeOperationConflictResult other)
        {
            return ConflictedFiles != null && other != null && ConflictedFiles.SequenceEqual(other.ConflictedFiles);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MergeOperationConflictResult) obj);
        }

    }
}