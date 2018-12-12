using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kuvalda.Repository;
using Kuvalda.Tree;
using Kuvalda.Tree.Data;
using Newtonsoft.Json;

namespace SimpleKvd.CLI
{
    class Program
    {
        private static string KvdSystemDirectoryPath = ".skvd";
        
        public static IObjectStorage TreesStorage => new FileSystemObjectStorage(new FileSystem(), TreesPath);
        public static IObjectStorage TreeDiffsStorage => new FileSystemObjectStorage(new FileSystem(), TreeDiffsPath);
        public static IObjectStorage ModHashesStorage => new FileSystemObjectStorage(new FileSystem(), ModificationHashesPath);
        public static IObjectStorage BlobsStorage => new FileSystemObjectStorage(new FileSystem(), BlobsPath);

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
            
            if (args[0] == "save-blob")
            {
                Console.Write(await SaveBlob(args[1]));
                return;
            }

            if (args[0] == "create-tree")
            {
                await CreateTree(args.Skip(1).ToArray());
                return;
            }

            if (args[0] == "save-tree")
            {
                Console.Write(await SaveTree(args.Skip(1).ToArray()));
                return;
            }
            
            if (args[0] == "save-init-tree")
            {
                Console.Write(await SaveEmptyTree());
                return;
            }
            
            if (args[0] == "diff-trees")
            {
                await DiffTreesToStdout(args.Skip(1).ToArray());
                return;
            }
            
            if (args[0] == "save-diff-trees")
            {
                Console.Write(await SaveDiffTrees(args.Skip(1).ToArray()));
                return;
            }
            
            if (args[0] == "hash-diff")
            {
                await HashDiffToStdOut(args.Skip(1).ToArray());
                return;
            }
            
            if (args[0] == "save-hash-diff")
            {
                Console.Write(await SaveHashDiff(args.Skip(1).ToArray()));
                return;
            }
            
            if (args[0] == "save-blobs-hashed")
            {
                var hashes = await SaveHashedBlobs(args.Skip(1).ToArray());
                foreach (var hash in hashes)
                {
                    Console.WriteLine(hash);
                }
                return;
            }

            Console.WriteLine("command not recognized!");
            Console.WriteLine("");
            PrintUsage();
            Environment.ExitCode = 1;
        }

        private static async Task<IEnumerable<string>> SaveHashedBlobs(string[] args)
        {
            var treeLeftStream = ModHashesStorage.Get(args[0]);

            IDictionary<string, string> hashes = null;

            using (var textStream = new StreamReader(treeLeftStream))
            {
                var data = await textStream.ReadToEndAsync();
                hashes = JsonConvert.DeserializeObject<IDictionary<string, string>>(data, new TreeNodeConverter());
            }

            var saveTasks = new List<Task<string>>();
            
            foreach (var hashRow in hashes)
            {
                saveTasks.Add(SaveBlob(hashRow.Key, hashRow.Value));
            }

            await Task.WhenAll(saveTasks);

            return saveTasks.Select(t => t.Result);
        }

        private static async Task<string> SaveBlob(string path, string hashPredefined = null)
        {
            EnsureSystemFolderCreated();

            var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var hash = hashPredefined ?? Hash(file);
            BlobsStorage.Set(hash, file);
            return hash;
        }

        private static async Task<string> SaveEmptyTree()
        {
            EnsureSystemFolderCreated();
            
            var tree = new TreeNodeFolder("");
            
            var hashString = SaveObjectToStorage(tree, TreesStorage);

            return hashString;
        }

        private static async Task<string> SaveHashDiff(string[] args)
        {
            var hashes = await HashModifications(args);
            
            var hashString = SaveObjectToStorage(hashes, ModHashesStorage);

            return hashString;
        }

        private static void PrintUsage()
        {
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"{nameof(CLI)}.usage.txt")))
            {
                Console.Write(stream.ReadToEnd());
                Console.WriteLine();
                return;
            }
        }

        private static async Task HashDiffToStdOut(string[] args)
        {
            var hashes = await HashModifications(args);

            Console.Write(JsonConvert.SerializeObject(hashes, Formatting.Indented));
        }

        private static async Task<IDictionary<string, string>> HashModifications(string[] args)
        {
            var hashComputer = new HashTableCreator(new FileSystem(), () => new SHA1Managed());
            var (leftTree, rightTree) = await GetTrees(args);
            var diff = DiffTrees(leftTree, rightTree);
            var rightFlat = new FlatTreeCreator().Create(rightTree);
            var forHashFlatItems = rightFlat.Where(i => diff.Modified.Contains(i.Name) || diff.Added.Contains(i.Name));
            var hashes = hashComputer.Compute(forHashFlatItems, Environment.CurrentDirectory);
            return hashes;
        }

        private static async Task<string> SaveDiffTrees(string[] args)
        {
            EnsureSystemFolderCreated();
            
            var diff = await DifferenceTrees(args);

            var hashString = SaveObjectToStorage(diff, TreeDiffsStorage);

            return hashString;
        }

        private static async Task DiffTreesToStdout(string[] args)
        {
            var result = await DifferenceTrees(args);

            Console.Write(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        private static async Task<DifferenceEntries> DifferenceTrees(string[] args)
        {
            var (leftTree, rightTree) = await GetTrees(args);
            var result = DiffTrees(leftTree, rightTree);            
            return result;
        }

        private static DifferenceEntries DiffTrees(TreeNode leftTree, TreeNode rightTree)
        {
            var flatTreeCreator = new FlatTreeCreator();
            var differ = new FlatTreeDiffer();
            var diffEntriesCreator = new DifferenceEntriesCreator(flatTreeCreator, differ);

            var result = diffEntriesCreator.Create(leftTree, rightTree);
            return result;
        }

        private static async Task<(TreeNode leftTree, TreeNode rightTree)> GetTrees(string[] args)
        {
            var treeLeftStream = TreesStorage.Get(args[0]);
            var treeRightStream = TreesStorage.Get(args[1]);

            TreeNode leftTree = null;
            TreeNode rightTree = null;

            using (var textStream = new StreamReader(treeLeftStream))
            {
                var data = await textStream.ReadToEndAsync();
                leftTree = JsonConvert.DeserializeObject<TreeNode>(data, new TreeNodeConverter());
            }

            using (var textStream = new StreamReader(treeRightStream))
            {
                var data = await textStream.ReadToEndAsync();
                rightTree = JsonConvert.DeserializeObject<TreeNode>(data, new TreeNodeConverter());
            }

            return (leftTree, rightTree);
        }

        private static async Task<string> SaveTree(string[] args)
        {
            EnsureSystemFolderCreated();
            
            var tree = await CreateTreeFiltered(Environment.CurrentDirectory);
            
            var hashString = SaveObjectToStorage(tree, TreesStorage);

            return hashString;
        }

        private static string SaveObjectToStorage(object tree, IObjectStorage storage)
        {
            var jsonRepresentation = JsonConvert.SerializeObject(tree, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(jsonRepresentation);
            string hashString = Hash(bytes);
            storage.Set(hashString, new MemoryStream(bytes));
            return hashString;
        }

        private static string Hash(byte[] bytes)
        {
            string hashString;
            using (var algorithm = new SHA1Managed())
            using (var stream = new MemoryStream(bytes))
            {
                var hash = algorithm.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                hashString = sb.ToString();
            }

            return hashString;
        }

        private static string Hash(Stream stream)
        {
            string hashString;
            using (var algorithm = new SHA1Managed())
            {
                var hash = algorithm.ComputeHash(stream);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                hashString = sb.ToString();
            }

            return hashString;
        }

        private static async Task CreateTree(string[] args)
        {
            var path = Environment.CurrentDirectory;
            if (args.Length == 1)
            {
                path = Path.GetFullPath(args[0]);
            }
            
            var treeFiltered = await CreateTreeFiltered(path);

            Console.Write(JsonConvert.SerializeObject(treeFiltered, Formatting.Indented));
        }

        
        private static async Task<TreeNode> CreateTreeFiltered(string path)
        {
            var fileSystem = new FileSystem();
            var treeCreator = new TreeCreator(fileSystem);
            var treeFilter = new TreeFilter(fileSystem);
            treeFilter.PredefinedIgnores = new List<string>
            {
                KvdSystemDirectoryPath
            };

            var tree = await treeCreator.Create(path);
            var treeFiltered = await treeFilter.Filter(tree, path);
            return treeFiltered;
        }

        private static void EnsureSystemFolderCreated()
        {
            if (!Directory.Exists(SystemPath))
            {
                Directory.CreateDirectory(SystemPath);
            }
        }
        
        public static string SystemPath => Path.Combine(Environment.CurrentDirectory, KvdSystemDirectoryPath);
        public static string TreesPath => Path.Combine(SystemPath, "trees");
        public static string TreeDiffsPath => Path.Combine(SystemPath, "diffs");
        public static string ModificationHashesPath => Path.Combine(SystemPath, "hashes");
        public static string BlobsPath => Path.Combine(SystemPath, "blobs");
    }
}