using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Tree
{
    public class TreeCreator : ITreeCreator
    {
        private readonly IFileSystem _fileSystem;

        public TreeCreator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task<TreeNode> Create(string path)
        {
            if (!_fileSystem.Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(path);
            }

            var folderTree = CreateFolder(path);

            var entryTask = _fileSystem.Directory.GetFileSystemEntries(path)
                .Select(async entry => await CreateNodeForEntry(entry));

            await Task.WhenAll(entryTask);

            folderTree.Nodes = entryTask.Select(t => t.Result);

            return folderTree;
        }

        public async Task<TreeNode> CreateNodeForEntry(string path)
        {
            if (_fileSystem.Directory.Exists(path))
            {
                return await Create(path);
            }

            return CreateFile(path);
        }

        private TreeNode CreateFile(string path)
        {
            return new TreeNodeFile(_fileSystem.Path.GetFileName(path), _fileSystem.File.GetLastWriteTimeUtc(path));
        }
        
        private TreeNodeFolder CreateFolder(string path)
        {
            return new TreeNodeFolder(_fileSystem.Path.GetFileName(path));
        }
    }
}