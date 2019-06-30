using System.Threading.Tasks;
using Kuvalda.Core;

namespace Kuvalda.Cli
{
    public class InitCommand : ICliCommand
    {
        private readonly IRepositoryFacade _repositoryFacade;
        private readonly ApplicationInstanceSettings _applicationInstanceSettings;

        public InitCommand(IRepositoryFacade repositoryFacade, ApplicationInstanceSettings applicationInstanceSettings)
        {
            _repositoryFacade = repositoryFacade;
            _applicationInstanceSettings = applicationInstanceSettings;
        }

        public async Task<int> Execute(string[] args)
        {
            await _repositoryFacade.Initialize(_applicationInstanceSettings.RepositoryPath);
            return 0;
        }
        
        public string GetHelp() => "initialize repository";
    }
}