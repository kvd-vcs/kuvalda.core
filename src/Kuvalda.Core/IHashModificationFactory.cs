using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface IHashModificationFactory
    {
        Task<IDictionary<string, string>> CreateHashes(TreeNode lTree, TreeNode rTree);
    }
}