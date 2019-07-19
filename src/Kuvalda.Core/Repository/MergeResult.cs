using System.Collections.Generic;
using Kuvalda.Core.Merge;

namespace Kuvalda.Core
{
    public abstract class MergeResult
    {
    }

    public class MergeResultSuccess : MergeResult
    {
        public string MergeCommitHash { get; set; }
    }

    public class MergeResultConflicts : MergeResult
    {
        public IEnumerable<MergeConflict> Conflicts { get; set; }
    }

    public class MergeResultInconsistentTree : MergeResult
    {
    }
}