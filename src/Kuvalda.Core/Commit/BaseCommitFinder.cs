using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class BaseCommitFinder : IBaseCommitFinder
    {
        private readonly IEntityObjectStorage<CommitModel> _commitEntity;

        public BaseCommitFinder(IEntityObjectStorage<CommitModel> commitEntity)
        {
            _commitEntity = commitEntity ?? throw new ArgumentNullException(nameof(commitEntity));
        }

        public async Task<string> FindBase(string leftCHash, string rightCHash)
        {
            if (string.IsNullOrEmpty(leftCHash))
            {
                throw new ArgumentNullException(nameof(leftCHash));
            }
            
            if (string.IsNullOrEmpty(rightCHash))
            {
                throw new ArgumentNullException(nameof(rightCHash));
            }
            
            var heap = new HashSet<string>();
            var leftQueue = new Queue<string>(new []{leftCHash});
            var rightQueue = new Queue<string>(new []{rightCHash});

            while (leftQueue.Any() || rightQueue.Any())
            {
                var commit = await ProcessCommit(leftQueue, heap);
                if (commit != null)
                {
                    return commit;
                }
                
                commit = await ProcessCommit(rightQueue, heap);
                if (commit != null)
                {
                    return commit;
                }
            }

            return null;
        }
        
        private async Task<string> ProcessCommit(Queue<string> queue, HashSet<string> heap)
        {
            if (!queue.Any())
            {
                return null;
            }
                
            var currentCHash = queue.Dequeue();
            if (heap.Contains(currentCHash))
            {
                return currentCHash;
            }

            heap.Add(currentCHash);
            var currentCommit = await _commitEntity.Get(currentCHash);
            foreach (var commit in currentCommit.Parents)
            {
                queue.Enqueue(commit);
            }

            return null;
        }
    }
}