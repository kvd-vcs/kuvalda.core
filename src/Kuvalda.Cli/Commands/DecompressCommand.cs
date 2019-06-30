using System;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;
using Serilog;

namespace Kuvalda.Cli
{
    public class DecompressCommand : ICliCommand
    {
        private readonly IRepositoryCompressFacade _repositoryCompressFacade;
        private readonly ApplicationInstanceSettings _settings;
        private readonly ILogger _logger;

        public DecompressCommand(IRepositoryCompressFacade repositoryCompressFacade, ApplicationInstanceSettings settings, ILogger logger)
        {
            _repositoryCompressFacade = repositoryCompressFacade ?? throw new ArgumentNullException(nameof(repositoryCompressFacade));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<int> Execute(string[] args)
        {
            if (args.Length < 2)
            {
                _logger.Fatal("Require patch hash parameter");
                return 1;
            }

            var patchHash = args.Skip(1).First();

            var result = await _repositoryCompressFacade.Patch(new PatchOptions
            {
                PatchHash = patchHash,
                RepositoryPath = _settings.RepositoryPath
            });

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return 0;
        }
        
        public string GetHelp() => "<patch hash> - apply patch";
    }
}