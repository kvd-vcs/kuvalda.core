namespace Kuvalda.Core
{
    public class CheckoutOptions
    {
        public readonly string CommitHash;

        public CheckoutOptions(string commitHash)
        {
            CommitHash = commitHash;
        }
    }
}