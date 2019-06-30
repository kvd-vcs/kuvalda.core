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
            Console.WriteLine("available commands:\n");

            foreach (var commands in _cliCommands)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(string.Format(" * {0,-15}", commands.Key));
                Console.ResetColor();
                Console.WriteLine(commands.Value.GetHelp());
            }
            
            Console.WriteLine();
            
            return Task.FromResult(0);
        }

        public string GetHelp() => "this help text";
    }
}