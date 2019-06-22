using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class HashTableCreator : IHashTableCreator
    {
        private readonly IFileSystem _fs;
        private readonly IHashComputeProvider _hasher;

        public HashTableCreator(IFileSystem fs, IHashComputeProvider hasher)
        {
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public async Task<IDictionary<string, string>> Compute(IEnumerable<FlatTreeItem> items, string context = "")
        {
            var withoutFolder = items.Where(i => !(i.Node is TreeNodeFolder));

            var tasks = withoutFolder.Select(f => Task.Run(async () =>
            {
                var filePath = _fs.Path.Combine(context, f.Name);
                var fileStream = _fs.File.OpenRead(filePath);

                var hash = await _hasher.Compute(fileStream);

                fileStream.Close();
                fileStream.Dispose();

                return (f.Name, hash);
            })).ToList();

            await Task.WhenAll(tasks);
            
            return new SortedDictionary<string, string>(tasks.ToDictionary(f => f.Result.Name, f => f.Result.hash));
        }
    }
}