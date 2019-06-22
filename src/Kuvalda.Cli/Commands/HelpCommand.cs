using System;
using System.Threading.Tasks;

namespace Kuvalda.Cli
{
    internal class HelpCommand : ICliCommand
    {
        public async Task<int> Execute(string[] args)
        {
            Console.WriteLine("usage: skvd [options] command [command options]");
            return 0;
        }
    }
}