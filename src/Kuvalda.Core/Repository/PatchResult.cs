using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class PatchResult
    {
        public string Method { get; set; }
        public IEnumerable<string> PatchedFiles { get; set; }
    }
}