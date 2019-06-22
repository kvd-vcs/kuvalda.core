using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class HashModificationFactory : IHashModificationFactory
    {
        public readonly IDifferenceEntriesCreator Differ;
        public readonly IFlatTreeCreator FlatTreeCreator;
        public readonly IHashTableCreator HashTableCreator;

        public HashModificationFactory(IDifferenceEntriesCreator differ, IFlatTreeCreator flatTreeCreator, IHashTableCreator hashTableCreator)
        {
            Differ = differ;
            FlatTreeCreator = flatTreeCreator;
            HashTableCreator = hashTableCreator;
        }

        public async Task<IDictionary<string, string>> CreateHashes(TreeNode lTree, TreeNode rTree)
        {
            var rightFlat = FlatTreeCreator.Create(rTree);
            var diff = Differ.Create(lTree, rTree);
            var forHashFlatItems = rightFlat.Where(i => diff.Modified.Contains(i.Name) || diff.Added.Contains(i.Name));
            var hashes = await HashTableCreator.Compute(forHashFlatItems, Environment.CurrentDirectory);

            return hashes;
        }
    }
}