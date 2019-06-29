using System.IO;

namespace Kuvalda.Core
{
    public interface ITempObjectStorage
    {
        Stream CreateTemp();
    }
}