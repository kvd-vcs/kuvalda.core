using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface ITreeCreator
    {
        Task<TreeNode> Create(string path);
    }
}