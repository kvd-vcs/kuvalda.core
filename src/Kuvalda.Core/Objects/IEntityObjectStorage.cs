using System.Threading.Tasks;
using Kuvalda.Core.Exceptions;

namespace Kuvalda.Core
{
    public interface IEntityObjectStorage<TEntity>
    {
        Task<bool> IsExists(string key);
        
        /// <summary>
        ///     Find entity in storage and return the object
        /// </summary>
        /// <param name="key"> key for entity </param>
        /// <returns> object of entity </returns>
        /// <exception cref="ObjectNotFoundException"> If entity with key not found in storage </exception>
        Task<TEntity> Get(string key);
        
        Task<string> Store(TEntity entity);
    }
}