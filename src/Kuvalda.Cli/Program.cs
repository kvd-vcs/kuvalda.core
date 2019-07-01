using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Kuvalda.Cli
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            await new Program(args).Run();
        }

        private async Task Run()
        {
            await ConfigureServices()
                .GetService<IStartup>()
                .Run(_args);
        }
    }
}