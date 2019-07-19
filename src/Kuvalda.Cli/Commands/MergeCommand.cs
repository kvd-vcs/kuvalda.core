using System;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;
using Serilog;

namespace Kuvalda.Cli
{
    public class MergeCommand : ICliCommand
    {
        private readonly IRepositoryMergeService _mergeService;
        private readonly ILogger _logger;

        public MergeCommand(IRepositoryMergeService mergeService, ILogger logger)
        {
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Execute(string[] args)
        {
            if (args.Length != 3)
            {
                _logger.Fatal("required 2 arguments");
                return 1;
            }

            var leftChash = args[1];
            var rightChash = args[2];
            var options = new MergeOptions()
            {
                LeftCHash = leftChash,
                RightCHash = rightChash
            };

            var result = await _mergeService.Merge(options);

            if (result is MergeResultSuccess success)
            {
                Console.WriteLine(success.MergeCommitHash);
                return 0;
            }

            _logger.Error("Merge error {@result}", result);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return 2;
        }

        public string GetHelp()
        {
            return
                "<left chash> <right chash> - try to merge passed left and right commits. Returns merge commit hash or errors";
        }
    }
}