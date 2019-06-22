using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class LogResult
    {
        public IEnumerable<LogEntry> Entries { get; set; }
    }

    public class LogEntry
    {
        public string CHash { get; set; }
        public IDictionary<string, string> Labels { get; set; }
        public string[] Parents { get; set; }
    }
}