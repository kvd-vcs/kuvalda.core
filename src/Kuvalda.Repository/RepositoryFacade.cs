using System.Collections.Generic;
using System.Threading.Tasks;
using Kuvalda.Tree;

namespace Kuvalda.Repository
{
    public class RepositoryFacade : IRepositoryFacade
    {
        public readonly IEntityObjectStorage<CommitModel> CommitStorage;
        public readonly IEntityObjectStorage<TreeNode> TreeStorage;
        public readonly IEntityObjectStorage<IDictionary<string, string>> HashStorage;
        public readonly IRefsStorage RefsStorage;

        public RepositoryFacade(IEntityObjectStorage<CommitModel> commitStorage,
            IEntityObjectStorage<TreeNode> treeStorage,
            IEntityObjectStorage<IDictionary<string, string>> hashStorage,
            IRefsStorage refsStorage)
        {
            CommitStorage = commitStorage;
            TreeStorage = treeStorage;
            HashStorage = hashStorage;
            RefsStorage = refsStorage;
        }

        public Task Initialize()
        {
            throw new System.NotImplementedException();
        }

        public Task<CommitResult> Commit(CommitOptions options)
        {
            throw new System.NotImplementedException();
        }

        public Task<CheckoutResult> Checkout(CheckoutOptions options)
        {
            throw new System.NotImplementedException();
        }

        public Task<StatusResult> GetStatus()
        {
            throw new System.NotImplementedException();
        }
    }
}