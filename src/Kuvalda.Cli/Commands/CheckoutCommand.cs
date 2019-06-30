using System;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;
using Serilog;

namespace Kuvalda.Cli
{
    public class CheckoutCommand : ICliCommand
    {
        private readonly IRepositoryFacade _repositoryFacade;
        private readonly ApplicationInstanceSettings _settings;
        private readonly ILogger _logger;

        public CheckoutCommand(IRepositoryFacade repositoryFacade, ApplicationInstanceSettings settings, ILogger logger)
        {
            _repositoryFacade = repositoryFacade ?? throw new ArgumentNullException(nameof(repositoryFacade));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Execute(string[] args)
        {
            var chash = args.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(chash))
            {
                _logger.Fatal("Commit hash required");
                return 1;
            }

            var result = await _repositoryFacade.Checkout(new CheckoutOptions(chash, _settings.RepositoryPath));
            
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            
            return 0;
        }

        public string GetHelp() => "<commitHash> - checkout to commitHash";
    }
}