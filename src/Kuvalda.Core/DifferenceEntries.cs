using System.Collections.Generic;

namespace Kuvalda.Core
{
    public class DifferenceEntries
    {
        public IEnumerable<string> Added;
        public IEnumerable<string> Modified;
        public IEnumerable<string> Removed;

        public DifferenceEntries(IEnumerable<string> added, IEnumerable<string> modified, IEnumerable<string> removed)
        {
            Added = added;
            Modified = modified;
            Removed = removed;
        }
    }
}