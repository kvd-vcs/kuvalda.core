using System.IO;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IHashComputeProvider
    {
        Task<string> Compute(Stream stream);
    }
}