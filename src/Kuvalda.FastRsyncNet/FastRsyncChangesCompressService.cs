using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastRsync.Core;
using FastRsync.Delta;
using FastRsync.Diagnostics;
using FastRsync.Signature;
using Kuvalda.Core;
using Serilog;

namespace Kuvalda.FastRsyncNet
{
    public class FastRsyncChangesCompressService : IChangesCompressService
    {
        public const string DIFF_METHOD = "frsync";
        
        private readonly IEntityObjectStorage<CommitModel> _commitStorage;
        private readonly IDifferenceEntriesCreator _differenceEntries;
        private readonly IEntityObjectStorage<TreeNode> _treeStorage;
        private readonly IObjectStorage _objectStorage;
        private readonly IFlatTreeCreator _flatTreeCreator;
        private readonly IHashComputeProvider _hashComputeProvider;
        private readonly ILogger _logger;

        public FastRsyncChangesCompressService(IEntityObjectStorage<CommitModel> commitStorage, IDifferenceEntriesCreator differenceEntries,
            IEntityObjectStorage<TreeNode> treeStorage, IObjectStorage objectStorage, IFlatTreeCreator flatTreeCreator,
            IHashComputeProvider hashComputeProvider, ILogger logger)
        {
            _commitStorage = commitStorage ?? throw new ArgumentNullException(nameof(commitStorage));
            _differenceEntries = differenceEntries ?? throw new ArgumentNullException(nameof(differenceEntries));
            _treeStorage = treeStorage ?? throw new ArgumentNullException(nameof(treeStorage));
            _objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
            _flatTreeCreator = flatTreeCreator ?? throw new ArgumentNullException(nameof(flatTreeCreator));
            _hashComputeProvider = hashComputeProvider ?? throw new ArgumentNullException(nameof(hashComputeProvider));
            _logger = logger;
        }

        public async Task<CompressModel> Compress(string srcCHash, string dstCHash)
        {
            var srcCommit = await _commitStorage.Get(srcCHash);
            var dstCommit = await _commitStorage.Get(dstCHash);

            var srcTree = await _treeStorage.Get(srcCommit.TreeHash);
            var dstTree = await _treeStorage.Get(dstCommit.TreeHash);

            var diff = _differenceEntries.Create(srcTree, dstTree);
            
            var flatTreeSrc = _flatTreeCreator.Create(srcTree).ToDictionary(i => i.Name, i => i.Node);
            var flatTreeDst = _flatTreeCreator.Create(dstTree).ToDictionary(i => i.Name, i => i.Node);

            var diffTasks = diff.Modified
                .Select(async modify => await CompressFile(modify, flatTreeSrc, flatTreeDst))
                .ToList();

            await Task.WhenAll(diffTasks);

            var deltas = diffTasks.Select(h => h.Result)
                .Where(i => i.hash != null)
                .ToDictionary(i => i.file, i => new CompressModel.DeltaInfo{ DeltaHash = i.hash, FileInfo = i.node });

            return new CompressModel()
            {
                Method = DIFF_METHOD,
                Source = srcCHash,
                Destination = dstCHash,
                Deltas = deltas
            };
        }

        private async Task<(string file, string hash, TreeNodeFile node)> CompressFile(string file, Dictionary<string, TreeNode> flatTreeSrc, Dictionary<string, TreeNode> flatTreeDst)
        {
            var srcNode = flatTreeSrc[file];
            var dstNode = flatTreeDst[file];

            if (srcNode is TreeNodeFolder)
            {
                return (file, null, null);
            }

            if (srcNode.GetType() != dstNode.GetType())
            {
                _logger?.Error("Inconsistent diff tree nodes. src: {@Src}, dst: {@Dst}", srcNode, dstNode);
                return (file, null, null);
            }
            
            _logger?.Debug("Begin check delta for file {file}", file);

            var srcNodeFile = srcNode as TreeNodeFile;
            var dstNodeFile = dstNode as TreeNodeFile;

            var srcStream = _objectStorage.Get(srcNodeFile.Hash);
            var dstStream = _objectStorage.Get(dstNodeFile.Hash);

            using (srcStream)
            using (dstStream)
            using (var signatureStream = new MemoryStream())
            {
                var signatureBuilder = new SignatureBuilder();
                await signatureBuilder.BuildAsync(srcStream, new SignatureWriter(signatureStream));

                signatureStream.Seek(0, SeekOrigin.Begin);

                var delta = new DeltaBuilder();
                using (var deltaStream = new MemoryStream())
                {
                    delta.BuildDelta(dstStream, new SignatureReader(signatureStream, new Progress<ProgressReport>()),
                        new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
                    
                    _logger?.Debug("Build delta finish for file {file}", file);
                    
                    deltaStream.Seek(0, SeekOrigin.Begin);
                    var hash = await _hashComputeProvider.Compute(deltaStream);
                    _objectStorage.Set(hash, deltaStream);
                    
                    _logger?.Debug("Delta for file {file} stored as {blob} blob", file, hash);
                    
                    return (file, hash, dstNodeFile);
                }
            }
        }
    }
}