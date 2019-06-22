using System;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;

namespace Kuvalda.Cli
{
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
            
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            
            return 0;
        }
    }
}