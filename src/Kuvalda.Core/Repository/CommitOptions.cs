namespace Kuvalda.Core
{
    public class CommitOptions
    {
        public readonly string Message;
        public readonly string Path;

        public CommitOptions(string path, string message)
        {
            Message = message;
            Path = path;
        }
    }
}