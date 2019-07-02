using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Serilog;

namespace Kuvalda.Core
{
    public class KeyValueStorage : IKeyValueStorage
    {
        private readonly IFileSystem _fs;
        private readonly ILogger _logger;
        private readonly RepositoryOptions _options;

        private const string KEYS_FOLDER_NAME = "keys";
        private string Path => _fs.Path.Combine(_options.SystemFolderPath, KEYS_FOLDER_NAME);

        public KeyValueStorage(IFileSystem fs, ILogger logger, RepositoryOptions options)
        {
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));
            _logger = logger;
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<string> Get(string key)
        {
            var path = _fs.Path.Combine(Path, key);

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

            if (!_fs.Directory.Exists(Path))
            {
                _logger?.Debug("Create keys folder at path {path}", Path);
                _fs.Directory.CreateDirectory(Path);
            }
            
            var path = _fs.Path.Combine(Path, key);
            
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