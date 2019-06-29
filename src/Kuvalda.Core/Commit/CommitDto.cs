using System.Collections.Generic;
using Kuvalda.Core;

namespace Kuvalda.Core
{
    public class CommitDto
    {
        public string Path { get; set; }
        public CommitModel Commit { get; set; }
        public TreeNode Tree { get; set; }
        public IEnumerable<string> ItemsForWrite { get; set; }
    }
}