using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Kuvalda.Tree;
using Newtonsoft.Json;

namespace TreeCreatorCLI
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var fileSystem = new FileSystem();
            var treeCreator = new TreeCreator(fileSystem);
            var treeFilter = new TreeFilter(fileSystem);

            var tree = await treeCreator.Create(args[0]);

            var treeFiltered = await treeFilter.Filter(tree, Path.GetFullPath(args[0]));

            File.WriteAllText(args[1], JsonConvert.SerializeObject(treeFiltered, Formatting.Indented));
        }
    }
}