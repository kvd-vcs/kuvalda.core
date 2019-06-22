using System.Threading.Tasks;

namespace Kuvalda.Cli
{
    public interface ICliCommand
    {
        Task<int> Execute(string[] args);
    }
}