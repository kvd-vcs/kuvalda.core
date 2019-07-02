using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using FastRsync.Delta;
using FastRsync.Diagnostics;
using Kuvalda.Core;
using Serilog;

namespace Kuvalda.FastRsyncNet
{
    public class FastRsyncChangesDecompressService : IChangesDecompressService
    {
        private readonly IObjectStorage _objectStorage;
        private readonly ITempObjectStorage _tempObjectStorage;
        private readonly IFileSystem _fs;
        private readonly ILogger _logger;

        public FastRsyncChangesDecompressService(IObjectStorage objectStorage, IFileSystem fs,
            ITempObjectStorage tempObjectStorage, ILogger logger)
        {
            _objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));;
            _logger = logger;
            _tempObjectStorage = tempObjectStorage ?? throw new ArgumentNullException(nameof(tempObjectStorage));
        }

        public async Task Apply(CompressModel patch, string path)
        {
            if (patch.Method != FastRsyncChangesCompressService.DIFF_METHOD)
            {
                _logger?.Fatal("Compress method {method} not valid", patch.Method);
                throw new InvalidDataException($"Compress method {patch.Method} not valid");
            }
            
            var notExistsFiles = patch.Deltas
                .Keys
                .Where(file => !_fs.File.Exists(_fs.Path.Combine(path, file)))
                .ToList();

            if (notExistsFiles.Any())
            {
                foreach (var file in notExistsFiles)
                {
                    _logger?.Fatal("File {file} not exists", _fs.Path.Combine(path, file));
                }
                throw new FileNotFoundException();
            }

            var patchTasks = patch.Deltas.Select(async file => await PatchFile(path, file)).ToList();

            await Task.WhenAll(patchTasks);
        }

        private async Task PatchFile(string path, KeyValuePair<string, CompressModel.DeltaInfo> file)
        {
            var filePath = _fs.Path.Combine(path, file.Key);
            var delta = new DeltaApplier {SkipHashCheck = true};
            using (var basisStream = _fs.File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var deltaStream = await _objectStorage.Get(file.Value.DeltaHash))
            using (var tempFileStream = _tempObjectStorage.CreateTemp())
            {
                var deltaReader = new BinaryDeltaReader(deltaStream, new Progress<ProgressReport>());
                delta.Apply(basisStream, deltaReader, tempFileStream);
                _logger?.Debug("For file {file} created delta", filePath);
                basisStream.SetLength(0);
                tempFileStream.Seek(0, SeekOrigin.Begin);
                await tempFileStream.CopyToAsync(basisStream);
                await basisStream.FlushAsync();
                _logger?.Debug("For file {file} applied delta", filePath);

                var lastTimeWrite = file.Value.FileInfo.ModificationTime;
                _fs.File.SetLastWriteTimeUtc(filePath, lastTimeWrite);
                _logger?.Debug("For file {file} set last write time {time}", filePath, lastTimeWrite);
            }
        }
    }
}