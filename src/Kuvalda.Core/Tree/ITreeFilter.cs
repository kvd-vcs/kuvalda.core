using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public interface ITreeFilter
    {
        Task<TreeNode> Filter(TreeNode original, string path);
    }
}