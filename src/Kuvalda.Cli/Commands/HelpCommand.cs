using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kuvalda.Cli
{
    public class HelpCommand : ICliCommand
    {
        private readonly IDictionary<string, ICliCommand> _cliCommands;

        public HelpCommand(IDictionary<string, ICliCommand> cliCommands)
        {
            _cliCommands = cliCommands;
        }

        public Task<int> Execute(string[] args)
        {
            Console.WriteLine("usage: skvd [options] command [command options]");
            Console.WriteLine("available commands:");

            foreach (var commands in _cliCommands)
            {
                Console.WriteLine(string.Format("\t{0,-20}: {1}", commands.Key, commands.Value.GetHelp()));
            }
            
            return Task.FromResult(0);
        }

        public string GetHelp() => "this help text";
    }
}