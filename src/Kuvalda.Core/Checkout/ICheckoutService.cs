using System;
using System.Threading.Tasks;

namespace Kuvalda.Core.Checkout
{
    public interface ICheckoutService
    {
        Task<DifferenceEntries> Checkout(string path, string chash);
    }

    public class CheckoutDecompressService : ICheckoutService
    {
        private readonly ICheckoutService _service;
        private readonly IRepositoryCompressFacade _compressFacade;
        private readonly IRefsService _refsService;

        public CheckoutDecompressService(ICheckoutService service, IRefsService refsService, IRepositoryCompressFacade compressFacade)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _refsService = refsService ?? throw new ArgumentNullException(nameof(refsService));
            _compressFacade = compressFacade ?? throw new ArgumentNullException(nameof(compressFacade));
        }

        public async Task<DifferenceEntries> Checkout(string path, string chash)
        {
            var currentCommit = _refsService.GetHeadCommit();
            await _compressFacade.Patch(new PatchOptions()
            {
                RepositoryPath = path,
                DestinationCommit = chash
            });

            return await _service.Checkout(path, chash);
        }
    }
}