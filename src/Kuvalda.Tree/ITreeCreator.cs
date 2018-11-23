using System.Threading.Tasks;

namespace Kuvalda.Tree
{
    public interface ITreeCreator
    {
        Task<TreeNode> Create(string path);
    }
}