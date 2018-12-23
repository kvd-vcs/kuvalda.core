using System.Threading.Tasks;

namespace Kuvalda.Repository
{
    public interface IRepositoryFacade
    {
        Task Initialize();
        Task<CommitResult> Commit(CommitOptions options);
        Task<CheckoutResult> Checkout(CheckoutOptions options);
        Task<StatusResult> GetStatus();
    }
}