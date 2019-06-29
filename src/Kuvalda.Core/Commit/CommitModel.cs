using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class CommitModel
    {
        public string TreeHash { get; set; }
        public IDictionary<string, string> Labels { get; set; }
        public ICollection<string> Parents { get; set; }
    }
}