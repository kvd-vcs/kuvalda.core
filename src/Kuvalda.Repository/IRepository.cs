using System.Threading.Tasks;

namespace Kuvalda.Repository
{
    public interface IRepository
    {
        Task Initialize();
        Task<CommitResult> Commit(CommitOptions options);
        Task<CheckoutResult> Checkout(CheckoutOptions options);
    }
}