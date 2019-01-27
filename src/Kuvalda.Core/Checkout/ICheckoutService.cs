using System.Threading.Tasks;

namespace Kuvalda.Core.Checkout
{
    public interface ICheckoutService
    {
        Task<DifferenceEntries> Checkout(string path, string chash);
    }
}