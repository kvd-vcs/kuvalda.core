using System;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;
using Serilog;

namespace Kuvalda.Cli
{
    public class CompressCommand : ICliCommand
    {
        private readonly IRepositoryCompressFacade _repositoryCompressFacade;
        private readonly ILogger _logger;

        public CompressCommand(IRepositoryCompressFacade repositoryCompressFacade, ILogger logger)
        {
            _repositoryCompressFacade = repositoryCompressFacade ?? throw new ArgumentNullException(nameof(repositoryCompressFacade));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Execute(string[] args)
        {
            if (args.Length < 3)
            {
                _logger.Fatal("Require 2 parameters: source commit and destination commit hashes");
                return 1;
            }

            var srcCommit = args.Skip(1).First();
            var dstCommit = args.Skip(2).First();

            var result = await _repositoryCompressFacade.Compress(new CompressOptions
            {
                SourceCommitHash = srcCommit,
                DestinationCommitHash = dstCommit
            });

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return 0;
        }
        
        public string GetHelp() => "<source commit> <destination commit> - create patch from source to destination commits";
    }
}