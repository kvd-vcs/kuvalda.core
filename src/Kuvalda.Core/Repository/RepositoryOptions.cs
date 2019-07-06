namespace Kuvalda.Core
{
    /// <summary>
    ///     Options for kvd ecosystem
    /// </summary>
    public class RepositoryOptions
    {
        public string SystemFolderPath { get; set; }
        public string HeadFileName { get; set; }
        public string DefaultBranchName { get; set; }
        public string MessageLabel { get; set; }
    }
}