using System;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class FileSystemObjectStorage : IObjectStorage
    {
        public readonly IFileSystem _fileSystem;
        public string StoragePath { get; set; }

        public FileSystemObjectStorage(IFileSystem fileSystem, string storagePath)
        {
            _fileSystem = fileSystem;
            StoragePath = storagePath;
        }

        public Task<bool> Exist(string key)
        {
            return Task.FromResult(_fileSystem.File.Exists(ObjectPath(key)));
        }

        public Task<Stream> Get(string key)
        {
            var path = ObjectPath(key);
            if (!_fileSystem.File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            
            return Task.FromResult(_fileSystem.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public async Task Set(string key, Stream obj)
        {
            var objPath = ObjectPath(key);
            var objPreFolderPath = _fileSystem.Path.GetDirectoryName(objPath);

            if (!_fileSystem.Directory.Exists(objPreFolderPath))
            {
                _fileSystem.Directory.CreateDirectory(objPreFolderPath);
            }
            
            using (var fileStream = _fileSystem.File.Create(objPath))
            {
                obj.Seek(0, SeekOrigin.Begin);
                await obj.CopyToAsync(fileStream);
            }
        }

        private (string head, string tall) DivideHash(string hash)
        {
            return (hash.Substring(0, 2), hash.Substring(2));
        }

        private string DividedPathHash(string hash)
        {
            var (head, tall) = DivideHash(hash);
            return _fileSystem.Path.Combine(head, tall);
        }

        private string ObjectPath(string hash)
        {
            return _fileSystem.Path.Combine(StoragePath, DividedPathHash(hash));
        }
    }
}