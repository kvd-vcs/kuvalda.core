using System.Collections.Generic;
using Kuvalda.Tree;

namespace Kuvalda.Repository
{
    public class CommitDto
    {
        public string Path { get; set; }
        public CommitModel Commit { get; set; }
        public TreeNode Tree { get; set; }
        public IDictionary<string, string> Hashes { get; set; }
    }
}