using System.Threading.Tasks;

namespace Kuvalda.Tree
{
    public interface ITreeFilter
    {
        Task<TreeNode> Filter(TreeNode original, string path);
    }
}