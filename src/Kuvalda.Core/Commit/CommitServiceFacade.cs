using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class CommitServiceFacade : ICommitServiceFacade
    {
        private readonly ICommitGetService _commitGetService;
        private readonly ICommitCreateService _commitCreateService;
        private readonly ICommitStoreService _commitStoreService;

        public CommitServiceFacade(ICommitGetService commitGetService, ICommitCreateService commitCreateService, ICommitStoreService commitStoreService)
        {
            _commitGetService = commitGetService;
            _commitCreateService = commitCreateService;
            _commitStoreService = commitStoreService;
        }


        public async Task<CommitDto> GetCommit(string chash)
        {
            return await _commitGetService.GetCommit(chash);
        }

        public async Task<CommitDto> CreateCommit(string path, string prevChash)
        {
            return await _commitCreateService.CreateCommit(path, prevChash);
        }

        public async Task<string> StoreCommit(CommitDto commit)
        {
            return await _commitStoreService.StoreCommit(commit);
        }
    }
}