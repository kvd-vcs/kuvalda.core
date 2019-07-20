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

            _logger.Information("Start modify file system at path {Path}", path);
            _logger.Information("Difference summary: remove {Removed}, add {Adding}, modified {Modified}", diff.Removed.Count(), diff.Added.Count(),
                diff.Modified.Count());
            
            Remove(path, diff);

            await Add(path, diff, flatTargetTree);

            await ModifyFiles(path, diff, flatTargetTree);

            await _refsService.SetHead(new CommitReference(chash));

            return diff;
        }

        private async Task ModifyFiles(string path, DifferenceEntries diff, List<FlatTreeItem> flatTargetTree)
        {
            var flatTreeMap = flatTargetTree.ToDictionary(c => c.Name, c => c.Node);
            foreach (var modified in diff.Modified)
            {
                var node = flatTreeMap[modified];
                if (node is TreeNodeFile fileNode)
                {
                    var modifiedPath = _fileSystem.Path.Combine(path, modified);
                    using (var modifyStream =
                        _fileSystem.File.Open(modifiedPath, FileMode.Truncate, FileAccess.Write, FileShare.Write))
                    {
                        var source = await _blobStorage.Get(fileNode.Hash);
                        await source.CopyToAsync(modifyStream);
                        await modifyStream.FlushAsync();
                        modifyStream.Close();

                        _fileSystem.File.SetLastWriteTimeUtc(modifiedPath,
                            ((TreeNodeFile) flatTargetTree.First(t => t.Name == modified).Node).ModificationTime);

                        _logger.Information("Modified file {Modifies}", modifiedPath);
                    }
                }
            }
        }

        private async Task Add(string path, DifferenceEntries diff, List<FlatTreeItem> flatTargetTree)
        {
            var flatTreeMap = flatTargetTree.ToDictionary(c => c.Name, c => c.Node);
            var addFiltered = diff.Added.ToList();
            foreach (var adding in addFiltered)
            {
                var node = flatTreeMap[adding];
                var addingPath = _fileSystem.Path.Combine(path, adding);

                switch (node)
                {
                    case TreeNodeFile fileNode:
                    {
                        using (var addStream = _fileSystem.File.Create(addingPath))
                        {
                            var source = await _blobStorage.Get(fileNode.Hash);
                            await source.CopyToAsync(addStream);
                            await addStream.FlushAsync();
                            addStream.Close();

                            var addingNode = ((TreeNodeFile) flatTargetTree.First(t => t.Name == adding).Node);
                            _fileSystem.File.SetLastWriteTimeUtc(adding, addingNode.ModificationTime);
                        }

                        _logger.Information("File added {Added}", addingPath);
                        break;
                    }

                    case TreeNodeFolder _:
                        _fileSystem.Directory.CreateDirectory(addingPath);
                        _logger.Information("Created folder {Added}", addingPath);
                        break;
                }
            }
        }

        private void Remove(string path, DifferenceEntries diff)
        {
            foreach (var removed in diff.Removed.Reverse())
            {
                var addingPath = _fileSystem.Path.Combine(path, removed);
                
                if (_fileSystem.File.Exists(addingPath))
                {
                    _fileSystem.File.Delete(addingPath);
                    _logger.Information("File deleted {Removed}", addingPath);
                }
                else if (_fileSystem.Directory.Exists(addingPath))
                {
                    _fileSystem.Directory.Delete(addingPath);
                    _logger.Information("Directory deleted {Removed}", addingPath);
                }
                else
                {
                    _logger.Warning("File or directory not exists but expected exists. Path: {Path}", addingPath);
                }
            }
        }
    }
}