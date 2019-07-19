using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class MergeOptions
    {
        public string LeftCHash { get; set; }
        public string RightCHash { get; set; }
        public IDictionary<string, string> Labels { get; set; }
    }
}