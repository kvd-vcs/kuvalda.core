using System;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;

namespace Kuvalda.Cli
{
    public class LogCommand : ICliCommand
    {
        private readonly IRefsService _refsService;
        private readonly IRepositoryFacade _repositoryFacade;
        private readonly RepositoryOptions _options;

        public LogCommand(IRefsService refsService, IRepositoryFacade repositoryFacade, RepositoryOptions options)
        {
            _refsService = refsService ?? throw new ArgumentNullException(nameof(refsService));
            _repositoryFacade = repositoryFacade ?? throw new ArgumentNullException(nameof(repositoryFacade));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<int> Execute(string[] args)
        {
            var log = await _repositoryFacade.GetLog(new LogOptions(){Reference = _options.HeadFilePath});
            
            Console.WriteLine(JsonConvert.SerializeObject(log, Formatting.Indented));
            
            return 0;
        }
        
        public string GetHelp() => "show commit history from current HEAD commit";
    }
}