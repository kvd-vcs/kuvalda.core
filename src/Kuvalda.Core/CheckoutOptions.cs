namespace Kuvalda.Core
{
    public class CheckoutOptions
    {
        public readonly string RepositoryPath;
        public readonly string CommitHash;

        public CheckoutOptions(string commitHash, string repositoryPath)
        {
            CommitHash = commitHash;
            RepositoryPath = repositoryPath;
        }
    }
}