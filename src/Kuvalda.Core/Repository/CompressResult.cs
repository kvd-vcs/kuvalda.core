using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class CompressResult
    {
        public string PatchHash { get; set; }
        public string Method { get; set; }
        public IEnumerable<string> CompressedFiles { get; set; }
    }
}