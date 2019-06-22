using System.Threading.Tasks;

namespace Kuvalda.Core.Status
{
    public interface IStatusService
    {
        Task<DifferenceEntries> GetStatus(string path, string chash);
    }
}