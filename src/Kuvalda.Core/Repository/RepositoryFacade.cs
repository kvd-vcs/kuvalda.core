using System.Threading.Tasks;
using Kuvalda.Core.Checkout;
using Kuvalda.Core.Status;

namespace Kuvalda.Core
{
    public class RepositoryFacade : IRepositoryFacade
    {
        private readonly RepositoryOptions _repositoryOptions;
        private readonly IRepositoryInitializeService _initializeService;
        private readonly ICommitServiceFacade _commitService;
        private readonly IRefsService _refsService;
        private readonly ICheckoutService _checkoutService;
        private readonly IStatusService _statusService;

        public RepositoryFacade(IRepositoryInitializeService initializeService, ICommitServiceFacade commitService,
            IRefsService refsService, ICheckoutService checkoutService, IStatusService statusService,
            RepositoryOptions repositoryOptions)
        {
            _initializeService = initializeService;
            _commitService = commitService;
            _refsService = refsService;
            _checkoutService = checkoutService;
            _statusService = statusService;
            _repositoryOptions = repositoryOptions;
        }


        public bool IsInitialized(string path)
        {
            return _initializeService.IsInitialized(path);
        }

        public async Task Initialize(string path)
        {
            await _initializeService.Initialize(path);
        }

        public async Task<CommitResult> Commit(CommitOptions options)
        {
            var currentChash = _refsService.GetHeadCommit();
            var commitData = await _commitService.CreateCommit(options.Path, currentChash);
            commitData.Commit.Labels[_repositoryOptions.MessageLabel] = options.Message;
            var chash = await _commitService.StoreCommit(commitData);
            _refsService.SetHead(new CommitReference(chash));
            
            return new CommitResult()
            {
                Chash = chash
            };
        }

        public async Task<CheckoutResult> Checkout(CheckoutOptions options)
        {
            var result = await _checkoutService.Checkout(options.RepositoryPath, options.CommitHash);
            return new CheckoutResult()
            {
                Added = result.Added,
                Removed = result.Removed,
                Modified = result.Modified
            };
        }

        public async Task<StatusResult> GetStatus(StatusOptions options)
        {
            var result = await _statusService.GetStatus(options.RepositoryPath,_refsService.GetHeadCommit());
            return new StatusResult()
            {
                Added = result.Added,
                Removed = result.Removed,
                Modified = result.Modified
            };
        }
    }
}