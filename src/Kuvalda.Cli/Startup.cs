using Microsoft.Extensions.Logging;

namespace Kuvalda.Cli
{
    public class Startup : IStartup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(ILogger<Startup> logger)
        {
            _logger = logger;
        }

        public void Run(string[] args)
        {
        }
    }
}