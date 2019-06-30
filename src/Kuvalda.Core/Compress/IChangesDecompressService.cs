using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IChangesDecompressService
    {
        Task Apply(CompressModel patch, string path);
    }
}