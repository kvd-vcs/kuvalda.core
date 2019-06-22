using System.Threading.Tasks;

namespace Kuvalda.Cli
{
    public interface IStartup
    {
        Task Run(string[] args);
    }
}