using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kuvalda.Core;
using Kuvalda.Core.Data;
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
        public static IObjectStorage CommitsStorage => new FileSystemObjectStorage(new FileSystem(), CommitsPath);

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
                Console.Write(SaveEmptyTree());
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
            
            if (args[0] == "create-commit")
            {
                Console.Write(await CreateCommit(args.Skip(1).ToArray()));
                return;
            }
            
            if (args[0] == "create-init-commit")
            {
                Console.Write(CreateInitCommit());
                return;
            }
            
            if (args[0] == "checkout")
            {
                await Checkout(args.Skip(1).ToArray());
                return;
            }
            
            if (args[0] == "write-ref")
            {
                WriteRef(args.Skip(1).ToArray());
                return;
            }
            
            if (args[0] == "write-head")
            {
                WriteHead(args.Skip(1).ToArray());
                return;
            }

            if (args[0] == "get-head-commit")
            {
                var headCommit = GetHeadCommit();

                if (headCommit == null)
                {
                    Console.WriteLine("HEAD is inconsistency!");
                    Environment.ExitCode = 1;
                    return;
                }
                
                Console.Write(headCommit);
                return;
            }
            
            if (args[0] == "init")
            {
                Initialize();
                return;
            }
            
            if (args[0] == "commit")
            {
                Console.Write(await Commit(args.Skip(1).ToArray()));
                return;
            }
            
            if (args[0] == "log")
            {
                await WriteLog(args.Skip(1).ToArray());
                return;
            }
            
            if (args[0] == "status")
            {
                await Status();
                return;
            }

            Console.WriteLine("command not recognized!");
            Console.WriteLine("");
            PrintUsage();
            Environment.ExitCode = 1;
        }

        private static async Task Status()
        {
            var currentTree = await CreateTreeFiltered(Environment.CurrentDirectory);
            var lastCommit = await ReadFromStorage<CommitModel>(GetHeadCommit(), CommitsStorage);
            var lastTree = await ReadFromStorage<TreeNode>(lastCommit.TreeHash, TreesStorage);
            
            var diff = DiffTrees(lastTree, currentTree);

            if (diff.Added.Any())
            {
                Console.WriteLine("New files:");
                foreach (var addEntry in diff.Added)
                {
                    Console.WriteLine(" + " + addEntry);
                }
                Console.WriteLine();
            }
            
            if (diff.Modified.Any())
            {
                Console.WriteLine("Modified files:");
                foreach (var entry in diff.Modified)
                {
                    Console.WriteLine(" * " + entry);
                }
                Console.WriteLine();
            }
            
            if (diff.Removed.Any())
            {
                Console.WriteLine("Deleted files:");
                foreach (var entry in diff.Removed)
                {
                    Console.WriteLine(" - " + entry);
                }
                Console.WriteLine();
            }
        }

        private static async Task WriteLog(string[] args)
        {
            var chash = GetHeadCommit();

            if (args.Length == 1)
            {
                chash = args[0];
            }

            await LogCommitRecursive(chash);
        }

        private static async Task LogCommitRecursive(string chash)
        {
            var commit = await ReadFromStorage<CommitModel>(chash, CommitsStorage);
            var message = "";
            commit.Labels.TryGetValue("message", out message);
            if (message == null)
            {
                message = "";
            }
            
            Console.Write($"{chash} {message}");
            if (commit.Parents != null && commit.Parents.Any())
            {
                Console.WriteLine();
                await LogCommitRecursive(commit.Parents.First());
            }
        }

        private static async Task<string> Commit(string[] args)
        {
            EnsureSystemFolderCreated();

            var labels = new Dictionary<string, string>();
            
            if (args.Length == 2 && args[0] == "-m")
            {
                labels["message"] = args[1];
            }
            
            var chash = await CreateCommit(GetHeadCommit(), labels);

            WriteHead(new[] {chash});
            
            return chash;
        }

        private static void Initialize()
        {
            EnsureSystemFolderCreated();
            var initChash = CreateInitCommit();
            WriteRef(new []{"master", initChash});
            WriteHead(new[]{"master"});
        }

        private static string GetHeadCommit()
        {
            var headData = File.ReadAllText(HeadPath);
            var headRefPath = Path.Combine(RefsPath, headData);

            if (File.Exists(headRefPath))
            {
                return File.ReadAllText(headRefPath);
            }

            if (!CommitsStorage.Exist(headData))
            {
                return null;
            }

            return headData;
        }

        private static void WriteHead(string[] args)
        {
            EnsureSystemFolderCreated();

            var refPath = Path.Combine(RefsPath, args[0]);

            if (File.Exists(refPath))
            {
                File.WriteAllText(HeadPath, args[0]);
                return;
            }
            
            if (!CommitsStorage.Exist(args[0]))
            {
                Console.Write($"commit {args[0]} not exists");
                Environment.ExitCode = 1;
                return;
            }
            
            File.WriteAllText(HeadPath, args[0]);
        }

        private static void WriteRef(string[] args)
        {
            EnsureSystemFolderCreated();

            if (!Directory.Exists(RefsPath))
            {
                Directory.CreateDirectory(RefsPath);
            }

            var refPath = Path.Combine(RefsPath, args[0]);
            
            if (!CommitsStorage.Exist(args[1]))
            {
                Console.WriteLine($"commit {args[1]} not exists");
                Environment.ExitCode = 1;
                return;
            }

            if (File.Exists(refPath))
            {
                Console.WriteLine($"Overwrite exists ref");
            }
            
            File.WriteAllText(refPath, args[1]);
        }

        private static async Task Checkout(string[] args)
        {
            var targetCommit = await ReadFromStorage<CommitModel>(args[0], CommitsStorage);
            
            var currentTree = await CreateTreeFiltered(Environment.CurrentDirectory);
            var targetTree = await ReadFromStorage<TreeNode>(targetCommit.TreeHash, TreesStorage);

            var diff = DiffTrees(currentTree, targetTree);

            foreach (var removed in diff.Removed)
            {
                if (!File.Exists(removed))
                {
                    continue;
                }
                
                File.Delete(removed);
                Console.WriteLine($" - {removed}");
            }

            var targetHashes = await GetHashesForCommit(targetCommit);
            var treeFlatter = new FlatTreeCreator();
            var flatTargetTree = treeFlatter.Create(targetTree);

            foreach (var adding in diff.Added.Where(a => targetHashes.ContainsKey(a)))
            {
                using (var addStream = File.Create(adding))
                {
                    var source = BlobsStorage.Get(targetHashes[adding]);
                    await source.CopyToAsync(addStream);
                    await addStream.FlushAsync();
                    addStream.Close();
                    
                    File.SetLastWriteTimeUtc(adding, ((TreeNodeFile)flatTargetTree.First(t => t.Name == adding).Node).ModificationTime);
                    
                    Console.WriteLine($" + {adding}");
                }
            }
            
            foreach (var modified in diff.Modified.Where(a => targetHashes.ContainsKey(a)))
            {
                using (var modifyStream = File.Open(modified, FileMode.Truncate, FileAccess.Write, FileShare.Write))
                {
                    var source = BlobsStorage.Get(targetHashes[modified]);
                    await source.CopyToAsync(modifyStream);
                    await modifyStream.FlushAsync();
                    modifyStream.Close();
                    
                    File.SetLastWriteTimeUtc(modified, ((TreeNodeFile)flatTargetTree.First(t => t.Name == modified).Node).ModificationTime);
                    
                    Console.WriteLine($" * {modified}");
                }
            }
            
            WriteHead(new [] {args[0]});
        }

        private static string CreateInitCommit()
        {
            EnsureSystemFolderCreated();
            
            var tree = new TreeNodeFolder("");
            var treeHash = SaveObjectToStorage(tree, TreesStorage);

            var hashes = new Dictionary<string, string>();
            var hashAddress = SaveObjectToStorage(hashes, ModHashesStorage);

            var commitObject = new CommitModel()
            {
                Parents = null,
                Labels = new Dictionary<string, string>()
                {
                    ["message"] = "init"
                },
                HashesAddress = hashAddress,
                TreeHash = treeHash
            };

            return SaveObjectToStorage(commitObject, CommitsStorage);
        }

        private static async Task<string> CreateCommit(string[] args)
        {
            EnsureSystemFolderCreated();

            var chash = args[0];
            var labels = new Dictionary<string, string>();
            
            return await CreateCommit(chash, labels);
        }

        private static async Task<string> CreateCommit(string chash, Dictionary<string, string> labels)
        {
            var tree = await CreateTreeFiltered(Environment.CurrentDirectory);
            var treeHash = SaveObjectToStorage(tree, TreesStorage);

            var prevCommit = await ReadFromStorage<CommitModel>(chash, CommitsStorage);

            var hashes = await HashModifications(new[] {prevCommit.TreeHash, treeHash});
            var hashAddress = SaveObjectToStorage(hashes, ModHashesStorage);

            await SaveHashedBlobs(hashes);

            var commitObject = new CommitModel()
            {
                Parents = new[] {chash},
                Labels = labels,
                HashesAddress = hashAddress,
                TreeHash = treeHash
            };

            return SaveObjectToStorage(commitObject, CommitsStorage);
        }

        private static async Task<IEnumerable<string>> SaveHashedBlobs(string[] args)
        {
            var hashes = await ReadFromStorage<IDictionary<string, string>>(args[0], ModHashesStorage);

            return await SaveHashedBlobs(hashes);
        }

        private static async Task<IEnumerable<string>> SaveHashedBlobs(IDictionary<string, string> hashes)
        {
            var result = new HashSet<string>();
            foreach (var hashRow in hashes)
            {
                result.Add(await SaveBlob(hashRow.Key, hashRow.Value));
            }
            return result;
        }

        private static async Task<string> SaveBlob(string path, string hashPredefined = null)
        {
            EnsureSystemFolderCreated();

            var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var hash = hashPredefined ?? Hash(file);
            await Task.Run(() => BlobsStorage.Set(hash, file));
            return hash;
        }

        private static string SaveEmptyTree()
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
                .GetManifestResourceStream("skvd.usage.txt")))
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

        private static async Task<T> ReadFromStorage<T>(string key, IObjectStorage strage, JsonConverter converter = null)
        {
            if (converter == null)
            {
                converter = new TreeNodeConverter();
            }
            
            var treeLeftStream = strage.Get(key);
            using (var textStream = new StreamReader(treeLeftStream))
            {
                var data = await textStream.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(data, converter);
            }
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
                File.WriteAllText(HeadPath, "");
            }
        }

        private static async Task<IDictionary<string, string>> GetHashesForCommit(CommitModel commit)
        {
            var result = await ReadFromStorage<IDictionary<string, string>>(commit.HashesAddress, ModHashesStorage);
            
            var prevCommits = commit.Parents?.Select(h =>
                ReadFromStorage<CommitModel>(h, CommitsStorage).GetAwaiter().GetResult()) ?? new List<CommitModel>();

            foreach (var prevCommit in prevCommits)
            {
                var prevState = await GetHashesForCommit(prevCommit);
                foreach (var prevRow in prevState)
                {
                    if (result.ContainsKey(prevRow.Key))
                    {
                        continue;
                    }
                    
                    result.Add(prevRow);
                }
            }

            return result;
        }
        
        public static string SystemPath => Path.Combine(Environment.CurrentDirectory, KvdSystemDirectoryPath);
        public static string TreesPath => Path.Combine(SystemPath, "trees");
        public static string TreeDiffsPath => Path.Combine(SystemPath, "diffs");
        public static string ModificationHashesPath => Path.Combine(SystemPath, "hashes");
        public static string BlobsPath => Path.Combine(SystemPath, "blobs");
        public static string CommitsPath => Path.Combine(SystemPath, "commits");
        public static string RefsPath => Path.Combine(SystemPath, "refs");
        public static string HeadPath => Path.Combine(SystemPath, "HEAD");
    }
}