using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IRepositoryFacade
    {
        /// <summary>
        ///     Return repository initialize status
        /// </summary>
        /// <returns> Initialization status </returns>
        bool IsInitialized(string path);
        
        /// <summary>
        ///     Initialize the repository
        /// </summary>
        Task Initialize(string path);
        
        /// <summary>
        ///     Create commit to repository with options
        /// </summary>
        /// <param name="options"> commit options </param>
        /// <returns> result dto of commit process </returns>
        Task<CommitResult> Commit(CommitOptions options);
        
        /// <summary>
        ///     Make checkout all state of repo to commit in options 
        /// </summary>
        /// <param name="options"> options for checkout process </param>
        /// <returns> result of checkout </returns>
        Task<CheckoutResult> Checkout(CheckoutOptions options);
        
        /// <summary>
        ///     Return status of repository(modified, deleted and added files)
        /// </summary>
        /// <returns> status of repo </returns>
        Task<StatusResult> GetStatus(StatusOptions options);
    }
}