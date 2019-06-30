using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Serilog;

namespace Kuvalda.Core
{
    public class KeyValueStorage : IKeyValueStorage
    {
        private readonly IFileSystem _fs;
        private readonly string _path;
        private readonly ILogger _logger;

        public KeyValueStorage(IFileSystem fs, string storagePath, ILogger logger)
        {
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));
            _logger = logger;
            _path = !string.IsNullOrEmpty(storagePath)
                ? storagePath
                : throw new ArgumentException(nameof(storagePath));
        }

        public async Task<string> Get(string key)
        {
            var path = _fs.Path.Combine(_path, key);

            if (!_fs.File.Exists(path))
            {
                _logger?.Debug("Key {key} not found", key);
                return null;
            }
            
            _logger?.Debug("Requested key {key}, path: {path}", key, path);
            
            return await Task.FromResult(_fs.File.ReadAllText(path));
        }

        public Task Set(string key, string value)
        {
            if (!_fs.Directory.Exists(_path))
            {
                _logger?.Debug("Create keys folder at path {path}", _path);
                _fs.Directory.CreateDirectory(_path);
            }
            
            var path = _fs.Path.Combine(_path, key);
            
            if (_fs.File.Exists(path))
            {
                _logger?.Debug("Key {key} overrided, path: {path}", key, path);
            }
            else
            {
                _logger?.Debug("Store new key {key}, path: {path}", key, path);
            }
            
            _fs.File.WriteAllText(path, value);

            return Task.CompletedTask;
        }
    }
}