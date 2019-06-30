using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Cli
{
    public class Startup : IStartup
    {
        private readonly IDictionary<string, ICliCommand> _commands;
        private readonly HelpCommand _helpCommand;

        public Startup(IDictionary<string, ICliCommand> commands, HelpCommand helpCommand)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _helpCommand = helpCommand ?? throw new ArgumentNullException(nameof(helpCommand));
        }

        public async Task Run(string[] args)
        {
            var commands = args.SkipWhile(a => a.StartsWith("--"));
            var commandName = commands.FirstOrDefault();

            ICliCommand command = null;
            if (commandName == null)
            {
                command = _helpCommand;
            }
            else
            {
                command = _commands[commandName];
            }
            
            System.Environment.ExitCode = await command.Execute(commands.ToArray());
        }
    }
}