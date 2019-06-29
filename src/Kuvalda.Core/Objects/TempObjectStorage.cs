using System;
using System.IO;
using System.IO.Abstractions;

namespace Kuvalda.Core
{
    public class TempObjectStorage : ITempObjectStorage
    {
        private readonly string _storagePath;
        private readonly IFileSystem _fs;

        public TempObjectStorage(string storagePath, IFileSystem fs)
        {
            _storagePath = storagePath;
            _fs = fs;
        }

        public Stream CreateTemp()
        {
            if (!_fs.Directory.Exists(_storagePath))
            {
                _fs.Directory.CreateDirectory(_storagePath);
            }
            
            var tempName = _fs.Path.Combine(_storagePath, Guid.NewGuid().ToString());
            var file = _fs.File.Create(tempName);
            return new TempFileStream(file, tempName, _fs);
        }
    }
}