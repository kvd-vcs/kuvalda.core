using System;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;

namespace Kuvalda.Cli
{
    public class CommitCommand : ICliCommand
    {
        private readonly IRepositoryFacade _repositoryFacade;
        private readonly ApplicationInstanceSettings _applicationInstanceSettings;

        public CommitCommand(IRepositoryFacade repositoryFacade, ApplicationInstanceSettings applicationInstanceSettings)
        {
            _repositoryFacade = repositoryFacade;
            _applicationInstanceSettings = applicationInstanceSettings;
        }

        public async Task<int> Execute(string[] args)
        {
            var message = args.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(message))
            {
                message = "no message";
            }

            var chash = await _repositoryFacade.Commit(new CommitOptions(_applicationInstanceSettings.RepositoryPath, message));

            Console.WriteLine(chash.Chash);
            
            return 0;
        }
        
        public string GetHelp() => "<commit message> - create and store commit with custom message";

    }
}