using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class CompressModel
    {
        public string Method { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public IDictionary<string, string> Deltas { get; set; }
    }
}