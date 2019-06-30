using System;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;

namespace Kuvalda.Cli
{
    public class StatusCommand : ICliCommand
    {
        private readonly IRepositoryFacade _repositoryFacade;
        private readonly ApplicationInstanceSettings _applicationInstanceSettings;

        public StatusCommand(IRepositoryFacade repositoryFacade, ApplicationInstanceSettings applicationInstanceSettings)
        {
            _repositoryFacade = repositoryFacade;
            _applicationInstanceSettings = applicationInstanceSettings;
        }

        public async Task<int> Execute(string[] args)
        {
            var status = await _repositoryFacade.GetStatus(new StatusOptions(_applicationInstanceSettings.RepositoryPath));

            Console.WriteLine(JsonConvert.SerializeObject(status, Formatting.Indented));
            
            return 0;
        }
        
        public string GetHelp() => "show difference from current HEAD commit";
    }
}