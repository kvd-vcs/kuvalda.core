using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Kuvalda.Core.Checkout
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ICommitGetService _getService;
        private readonly ITreeCreator _treeCreator;
        private readonly IDifferenceEntriesCreator _differenceEntriesCreator;
        private readonly IObjectStorage _blobStorage;
        private readonly IRefsService _refsService;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly IFlatTreeCreator _flatTreeCreator;

        public CheckoutService(ICommitGetService getService, ITreeCreator treeCreator,
            IDifferenceEntriesCreator differenceEntriesCreator, IObjectStorage blobStorage, IRefsService refsService,
            IFileSystem fileSystem, ILogger logger, IFlatTreeCreator flatTreeCreator)
        {
            _getService = getService;
            _treeCreator = treeCreator;
            _differenceEntriesCreator = differenceEntriesCreator;
            _blobStorage = blobStorage;
            _refsService = refsService;
            _fileSystem = fileSystem;
            _logger = logger;
            _flatTreeCreator = flatTreeCreator;
        }

        public async Task<DifferenceEntries> Checkout(string path, string chash)
        {
            var targetCommit = await _getService.GetCommit(chash);
            var currentTree = await _treeCreator.Create(path);
            var diff = _differenceEntriesCreator.Create(currentTree, targetCommit.Tree);
            var flatTargetTree = _flatTreeCreator.Create(targetCommit.Tree).ToList();
            var hashes = targetCommit.Hashes;

            _logger.Information("Start modify file system at path {Path}", path);
            _logger.Information("Difference summary: remove {Removed}, add {Adding}, modified {Modified}", diff.Removed.Count(), diff.Added.Count(),
                diff.Modified.Count());
            
            RemoveFiles(path, diff);

            await AddFiles(path, diff, hashes, flatTargetTree);

            await ModifyFiles(path, diff, hashes, flatTargetTree);

            _refsService.SetHeadCommit(chash);

            return diff;
        }

        private async Task ModifyFiles(string path, DifferenceEntries diff, IDictionary<string, string> hashes, List<FlatTreeItem> flatTargetTree)
        {
            foreach (var modified in diff.Modified.Where(a => hashes.ContainsKey(a)))
            {
                var modifiedPath = _fileSystem.Path.Combine(path, modified);
                using (var modifyStream =
                    _fileSystem.File.Open(modifiedPath, FileMode.Truncate, FileAccess.Write, FileShare.Write))
                {
                    var source = _blobStorage.Get(hashes[modified]);
                    await source.CopyToAsync(modifyStream);
                    await modifyStream.FlushAsync();
                    modifyStream.Close();

                    File.SetLastWriteTimeUtc(modifiedPath,
                        ((TreeNodeFile) flatTargetTree.First(t => t.Name == modified).Node).ModificationTime);

                    _logger.Information("Modified file {Modifies}", modifiedPath);
                }
            }
        }

        private async Task AddFiles(string path, DifferenceEntries diff, IDictionary<string, string> hashes, List<FlatTreeItem> flatTargetTree)
        {
            var addFiltered = diff.Added.Where(a => hashes.ContainsKey(a)).ToList();
            foreach (var adding in addFiltered)
            {
                var addingPath = _fileSystem.Path.Combine(path, adding);
                using (var addStream = _fileSystem.File.Create(addingPath))
                {
                    var source = _blobStorage.Get(hashes[adding]);
                    await source.CopyToAsync(addStream);
                    await addStream.FlushAsync();
                    addStream.Close();

                    var addingNode = ((TreeNodeFile) flatTargetTree.First(t => t.Name == adding).Node);
                    _fileSystem.File.SetLastWriteTimeUtc(adding, addingNode.ModificationTime);
                }

                _logger.Information("File added {Added}", addingPath);
            }
        }

        private void RemoveFiles(string path, DifferenceEntries diff)
        {
            foreach (var removed in diff.Removed)
            {
                var addingPath = _fileSystem.Path.Combine(path, removed);
                if (!_fileSystem.File.Exists(addingPath))
                {
                    _logger.Warning("File {Removed} mark for remove, but not exist", addingPath);
                    continue;
                }

                _fileSystem.File.Delete(addingPath);
                _logger.Information("File deleted {Removed}", addingPath);
            }
        }
    }
}