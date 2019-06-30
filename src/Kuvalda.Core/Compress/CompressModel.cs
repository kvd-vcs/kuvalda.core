using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class CompressModel
    {
        public string Method { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public IDictionary<string, DeltaInfo> Deltas { get; set; }

        public class DeltaInfo
        {
            public string DeltaHash { get; set; }
            public TreeNodeFile FileInfo { get; set; }
        }
    }
}