namespace Kuvalda.Core
{
    public class CommitReference : Reference
    {
        public CommitReference(string chash)
        {
            Value = chash;
        }
    }
}