using System.Collections.Generic;
using System.Threading.Tasks;
using Kuvalda.Tree;

namespace Kuvalda.Repository
{
    public interface IHashModificationFactory
    {
        Task<IDictionary<string, string>> CreateHashes(TreeNode lTree, TreeNode rTree);
    }
}