using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class LogService : ILogService
    {
        private readonly IRefsService _refsService;
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        
        public LogService(IRefsService refsService, IEntityObjectStorage<CommitModel> commitStorage)
        {
            _refsService = refsService ?? throw new ArgumentNullException(nameof(refsService));
            _commitStorage = commitStorage ?? throw new ArgumentNullException(nameof(commitStorage));
        }

        public async Task<LogResult> GetLog(LogOptions options)
        {
            var headCHash = _refsService.GetCommit(options.Reference);
            var queue = new Queue<string>(new []{headCHash});
            var logs = new List<LogEntry>();

            while (queue.Any())
            {
                var chash = queue.Dequeue();
                var commit = await _commitStorage.Get(chash);
                logs.Add(new LogEntry()
                {
                    Labels = commit.Labels,
                    CHash = chash,
                    Parents = commit.Parents.ToArray()
                });
                
                foreach (var parent in commit.Parents)
                {
                    queue.Enqueue(parent);
                }
            }

            return new LogResult()
            {
                Entries = logs
            };
        }
    }
}