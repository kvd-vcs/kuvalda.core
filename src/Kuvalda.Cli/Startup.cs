using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kuvalda.Cli
{
    public class Startup : IStartup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IDictionary<string, ICliCommand> _commands;

        public Startup(ILogger<Startup> logger, IDictionary<string, ICliCommand> commands)
        {
            _logger = logger;
            _commands = commands;
        }

        public async Task Run(string[] args)
        {
            var commands = args.SkipWhile(a => a.StartsWith("--"));
            var commandName = commands.FirstOrDefault();

            ICliCommand command = null;
            if (commandName == null)
            {
                command = _commands["help"];
            }
            else
            {
                command = _commands[commandName];
            }
            
            System.Environment.ExitCode = await command.Execute(commands.ToArray());
        }
    }
}