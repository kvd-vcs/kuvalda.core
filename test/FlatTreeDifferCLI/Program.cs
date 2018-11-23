using System.IO;
using System.Linq;
using Kuvalda.Tree;
using Kuvalda.Tree.Data;
using Newtonsoft.Json;

namespace FlatTreeDifferCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var flatTreeCreator = new FlatTreeCreator();
            var differ = new FlatTreeDiffer();
            var diffEntriesCreator = new DifferenceEntriesCreator(flatTreeCreator, differ);

            var treeLeft = JsonConvert.DeserializeObject<TreeNode>(File.ReadAllText(args[0]), new TreeNodeConverter());
            var treeRight = JsonConvert.DeserializeObject<TreeNode>(File.ReadAllText(args[1]), new TreeNodeConverter());

            var result = diffEntriesCreator.Create(treeLeft, treeRight);
            
            File.WriteAllText(args[2], JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}