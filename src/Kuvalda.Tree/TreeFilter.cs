using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kuvalda.Tree
{
    public class TreeFilter
    {
        public const string IgnoreFileName = ".kvdignore";

        private readonly IFileSystem _fileSystem;

        public TreeFilter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task<TreeNode> Filter(TreeNode original, string path)
        {
            if (original is TreeNodeFolder folder)
            {
                return await FilterFolder(folder, _fileSystem.Path.Combine(path, original.Name));
            }

            return original;
        }

        private async Task<TreeNode> FilterFolder(TreeNodeFolder folder, string contextPath)
        {
            if (folder.Nodes.Any(f => f.Name == IgnoreFileName))
            {
                var ignores = JsonConvert
                    .DeserializeObject<string[]>(await ReadAllTextAsync(_fileSystem.Path.Combine(contextPath, IgnoreFileName)))
                    .Select(s => new Regex(s));
                
                var result = new TreeNodeFolder(folder.Name);
                var filteredTasks = folder.Nodes
                    .Where(entry => !ignores.Any(matcher => matcher.IsMatch(entry.Name)))
                    .Select(async entry => await Filter(entry, contextPath));

                await Task.WhenAll(filteredTasks);

                result.Nodes = filteredTasks.Select(task => task.Result);

                return result;
            }

            return folder;
        }

        private async Task<string> ReadAllTextAsync(string path)
        {
            return await Task.Run(() => _fileSystem.File.ReadAllText(path));
        }
    }
}