using System;
using System.Threading.Tasks;

namespace Kuvalda.Core.Checkout
{
    public class CheckoutDecompressService : ICheckoutService
    {
        private readonly ICheckoutService _service;
        private readonly IRepositoryCompressFacade _compressFacade;

        public CheckoutDecompressService(ICheckoutService service, IRepositoryCompressFacade compressFacade)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _compressFacade = compressFacade ?? throw new ArgumentNullException(nameof(compressFacade));
        }

        public async Task<DifferenceEntries> Checkout(string path, string chash)
        {
            await _compressFacade.Patch(new PatchOptions()
            {
                RepositoryPath = path,
                DestinationCommit = chash
            });

            return await _service.Checkout(path, chash);
        }
    }
}