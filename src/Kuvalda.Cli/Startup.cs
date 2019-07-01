using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Kuvalda.Storage.Http;
using Serilog;

namespace Kuvalda.Cli
{
    public class Startup : IStartup
    {
        private readonly IDictionary<string, ICliCommand> _commands;
        private readonly HelpCommand _helpCommand;
        private readonly ILogger _logger;
        
        private readonly RepositoryOptions _options;
        private readonly ApplicationInstanceSettings _instanceOptions;
        private readonly EndpointOptions _endpointOptions;

        public Startup(IDictionary<string, ICliCommand> commands, HelpCommand helpCommand, ILogger logger,
            RepositoryOptions options, ApplicationInstanceSettings instanceOptions, EndpointOptions endpointOptions)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _helpCommand = helpCommand ?? throw new ArgumentNullException(nameof(helpCommand));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options;
            _instanceOptions = instanceOptions;
            _endpointOptions = endpointOptions;
        }

        public async Task Run(string[] args)
        {
            _logger.Debug("Begin application handle command. Arguments: {args}, available commands: {commands}", args, _commands.Keys);
            
            var commands = args.SkipWhile(a => a.StartsWith("--"));
            var commandName = commands.FirstOrDefault();
            
            _logger.Debug("Command name: {name}", commandName);
            
            _logger.Debug("Engine options: {@engineOptions}", _options);
            _logger.Debug("Instance options: {@instanceOptions}", _instanceOptions);
            _logger.Debug("Endpoint options: {@endpointOptions}", _endpointOptions);
            
            ICliCommand command = null;
            if (string.IsNullOrEmpty(commandName))
            {
                command = _helpCommand;
            }
            else if (_commands.ContainsKey(commandName))
            {
                command = _commands[commandName];
            }
            else
            {
                _logger.Fatal("Not found command {name}", commandName);
                Environment.ExitCode = 1;
                return;
            }

            _logger.Debug("Found command {commandType}, begin execution", command);

            try
            {
                Environment.ExitCode = await command.Execute(commands.ToArray());
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Unhandled Exception");
                Environment.ExitCode = 2;
            }
            
            _logger.Debug("Execution ended. Exit code is {exitCode}", Environment.ExitCode);
        }
    }
}