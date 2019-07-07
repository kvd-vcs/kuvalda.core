using System.Threading.Tasks;

namespace Kuvalda.Core
{
    /// <summary>
    ///     Find the base commit for left and right subtrees
    /// </summary>
    public interface IBaseCommitFinder
    {
        Task<string> FindBase(string leftCHash, string rightCHash);
    }
}