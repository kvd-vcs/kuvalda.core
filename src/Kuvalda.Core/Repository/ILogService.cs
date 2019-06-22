using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface ILogService
    {
       Task<LogResult> GetLog(LogOptions options);
    }
}