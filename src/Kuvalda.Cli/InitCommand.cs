using System;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;

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
    }
    
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

            Console.WriteLine(JsonConvert.SerializeObject(status));
            
            return 0;
        }
    }

    
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

            Console.WriteLine(chash);
            
            return 0;
        }
    }
    
    public class CheckoutCommand : ICliCommand
    {
        private readonly IRepositoryFacade _repositoryFacade;
        private readonly ApplicationInstanceSettings _applicationInstanceSettings;

        public CheckoutCommand(IRepositoryFacade repositoryFacade, ApplicationInstanceSettings applicationInstanceSettings)
        {
            _repositoryFacade = repositoryFacade;
            _applicationInstanceSettings = applicationInstanceSettings;
        }

        public async Task<int> Execute(string[] args)
        {
            var chash = args.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(chash))
            {
                Console.WriteLine("Chash required");
                return 1;
            }

            var result = await _repositoryFacade.Checkout(new CheckoutOptions(chash, _applicationInstanceSettings.RepositoryPath));
            
            Console.WriteLine(JsonConvert.SerializeObject(result));
            
            return 0;
        }
    }
}