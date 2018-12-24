using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class HashTableCreator : IHashTableCreator
    {
        private readonly IFileSystem _fs;
        private readonly Func<HashAlgorithm> _algorithmFactory;

        public HashTableCreator(IFileSystem fs, Func<HashAlgorithm> algorithmFactory)
        {
            if (fs == null)
            {
                throw new ArgumentNullException(nameof(fs));
            }
            
            if (algorithmFactory == null)
            {
                throw new ArgumentNullException(nameof(algorithmFactory));
            }
            
            _fs = fs;
            _algorithmFactory = algorithmFactory;
        }

        public IDictionary<string, string> Compute(IEnumerable<FlatTreeItem> items, string context = "")
        {
            var withoutFolder = items.Where(i => !(i.Node is TreeNodeFolder));
            var result = new ConcurrentDictionary<string, string>();
           
            Parallel.ForEach(withoutFolder, item =>
            {
                using (var algorithm = _algorithmFactory.Invoke())
                {
                    var filePath = _fs.Path.Combine(context, item.Name);
                    var fileStream = _fs.File.OpenRead(filePath);
                    
                    var hash = algorithm.ComputeHash(fileStream);
                    
                    fileStream.Close();
                    fileStream.Dispose();
                    
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("x2"));
                    }

                    result[item.Name] = sb.ToString();
                }
                
            });

            return new SortedDictionary<string, string>(result);
        }
    }
}