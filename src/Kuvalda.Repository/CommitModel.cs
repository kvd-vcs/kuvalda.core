using System.Collections.Generic;

namespace Kuvalda.Repository
{
    public class CommitModel
    {
        public string TreeHash { get; set; }
        public string HashesAddress { get; set; }
        public IDictionary<string, string> Labels { get; set; }
        public ICollection<string> Parents { get; set; }
    }
}