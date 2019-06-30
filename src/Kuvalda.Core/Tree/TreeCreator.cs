using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Kuvalda.Core
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
            var result = await CreateInterlal(path);
            return new TreeNodeFolder("")
            {
                Nodes = ((TreeNodeFolder)result).Nodes
            };
        }
        
        private async Task<TreeNode> CreateInterlal(string path)
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
                return await CreateInterlal(path);
            }

            return CreateFile(path);
        }

        private TreeNode CreateFile(string path)
        {
            var lastWriteTimeUtc = _fileSystem.File.GetLastWriteTimeUtc(path);
            lastWriteTimeUtc = lastWriteTimeUtc.AddMilliseconds(-lastWriteTimeUtc.Millisecond);
            
            return new TreeNodeFile(_fileSystem.Path.GetFileName(path), lastWriteTimeUtc);
        }
        
        private TreeNodeFolder CreateFolder(string path)
        {
            return new TreeNodeFolder(_fileSystem.Path.GetFileName(path));
        }
    }
}