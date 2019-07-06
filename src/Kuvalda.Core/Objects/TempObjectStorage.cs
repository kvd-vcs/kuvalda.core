using System;
using System.IO;
using System.IO.Abstractions;
using Serilog;

namespace Kuvalda.Core
{
    public class TempObjectStorage : ITempObjectStorage
    {
        private readonly IFileSystem _fs;
        private readonly ILogger _logger;
        private readonly RepositoryOptions _options;
        
        private const string TEMP_FOLDER_NAME = "temp";
        private string _path => _fs.Path.Combine(_options.SystemFolderPath, TEMP_FOLDER_NAME);

        public TempObjectStorage(IFileSystem fs, ILogger logger, RepositoryOptions options)
        {
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Stream CreateTemp()
        {
            if (!_fs.Directory.Exists(_path))
            {
                _fs.Directory.CreateDirectory(_path);
            }
            
            var tempName = _fs.Path.Combine(_path, Guid.NewGuid().ToString());
            var file = _fs.File.Create(tempName);
            return new TempFileStream(file, tempName, _fs, _logger);
        }
    }
}