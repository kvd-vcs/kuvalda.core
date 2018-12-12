using System.Collections.Generic;
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

        public IList<string> PredefinedIgnores { get; set; } 

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
            var result = (TreeNodeFolder)folder.Clone();
            
            if (PredefinedIgnores != null)
            {
                result.Nodes = result.Nodes.Where(t => !PredefinedIgnores.Contains(t.Name)).ToList();
            }
            
            if (result.Nodes.Any(f => f.Name == IgnoreFileName))
            {
                var ignores = JsonConvert
                    .DeserializeObject<string[]>(await ReadAllTextAsync(_fileSystem.Path.Combine(contextPath, IgnoreFileName)))
                    .Select(s => new Regex(s));
                
                var filteredTasks = result.Nodes
                    .Where(entry => !ignores.Any(matcher => matcher.IsMatch(entry.Name)))
                    .Select(async entry => await Filter(entry, contextPath));

                await Task.WhenAll(filteredTasks);

                result.Nodes = filteredTasks.Select(task => task.Result);

                return result;
            }

            return result;
        }

        private async Task<string> ReadAllTextAsync(string path)
        {
            return await Task.Run(() => _fileSystem.File.ReadAllText(path));
        }
    }
}