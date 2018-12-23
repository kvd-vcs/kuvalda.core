using System.IO;
using System.Threading.Tasks;

namespace Kuvalda.Repository
{
    public interface IHashComputeProvider
    {
        Task<string> Compute(Stream stream);
    }
}