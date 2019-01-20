using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class StatusResult
    {
        public IEnumerable<string> Added { get; set; }
        public IEnumerable<string> Removed { get; set; }
        public IEnumerable<string> Modified { get; set; }
    }
}