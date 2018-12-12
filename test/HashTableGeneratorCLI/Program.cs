using System;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Kuvalda.Tree;
using Newtonsoft.Json;

namespace HashTableGeneratorCLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var flatTreeCreator = new FlatTreeCreator();
            var fileSystem = new FileSystem();
            var treeCreator = new TreeCreator(fileSystem);
            var treeFilter = new TreeFilter(fileSystem);
            var hashComputer = new HashTableCreator(fileSystem, () => new SHA1Managed());

            var tree = await treeCreator.Create(args[0]);
            var treeFiltered = await treeFilter.Filter(tree, args[0]);
            var flatTree = flatTreeCreator.Create(treeFiltered);
            
            var hashes = hashComputer.Compute(flatTree, Environment.CurrentDirectory);
            
            File.WriteAllText(args[1], JsonConvert.SerializeObject(hashes, Formatting.Indented));
        }
    }
}