using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Kuvalda.Core
{
    public class RefsService : IRefsService
    {
        private readonly IFileSystem _fs;
        private readonly RepositoryOptions _options;
        private readonly ILogger _logger;
        
        public RefsService(IFileSystem fileSystem, RepositoryOptions options, ILogger logger)
        {
            _fs = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _options = options ?? throw new ArgumentNullException(nameof(options));;
            _logger = logger;
        }

        public async Task<bool> Exists(string name)
        {
            return _fs.File.Exists(_fs.Path.Combine(_options.SystemFolderPath, "refs", name));
        }

        public async Task<Reference> Get(string name)
        {
            var reference = await GetInternal(name);
            
            if (reference is EmptyReference)
            {
                throw new InvalidDataException($"Reference {name} is empty");
            }

            return reference;
        }
        
        public async Task<string> GetCommit(string name)
        {
            var reference = await GetInternal(name);
            
            switch (reference)
            {
                case EmptyReference _:
                    throw new InvalidDataException($"Reference {name} is empty");
                
                case CommitReference _:
                    return reference.Value;
                
                case PointerReference _:
                    return await GetCommit(reference.Value);
                
                default:
                    return reference.Value;
            }
        }

        public Task Store(string name, Reference value)
        {
            var prefix = value is PointerReference ? "ref" : "commit";
            var format = $"{prefix}:{value.Value}";
            _fs.File.WriteAllText(_fs.Path.Combine(_options.SystemFolderPath, "refs", name), format);
            _logger?.Debug("Stored reference {name} with data {data}", name, format);
            return Task.CompletedTask;
        }

        public async Task<string> GetHeadCommit()
        {
            var reference = await GetHead();

            if (!(reference is PointerReference p))
            {
                return reference.Value;
            }
            
            var chashRef = await Get(p.Value);
            if (chashRef == null)
            {
                throw new InvalidDataException($"Reference is null. Check db consistency");
            }
            
            return chashRef.Value;
        }
        
        public async Task SetHead(Reference value)
        {
            var head = await GetInternal(_options.HeadFileName);
            
            switch (head)
            {
                case null:
                case EmptyReference _:
                    await Store(_options.HeadFileName, value);
                    break;
                case PointerReference _ when !(value is PointerReference):
                    await Store(head.Value, value);
                    return;
            }

            await Store(_options.HeadFileName, value);
        }

        public Task<string[]> GetAll()
        {
            var path = _fs.Path.Combine(_options.SystemFolderPath, "refs");
            var files = _fs.Directory.GetFiles(path, "*",
                    SearchOption.AllDirectories)
                .Select(file => file.Replace(path, ""));
            return Task.FromResult(files.ToArray());
        }

        public async Task<Reference> GetHead()
        {
            var reference = await GetInternal(_fs.Path.Combine(_options.SystemFolderPath, "refs", _options.HeadFileName));
            if (reference == null)
            {
                throw new FileNotFoundException($"Head reference not found. Check db consistency");
            }

            return reference;
        }
        
        private async Task<Reference> GetInternal(string name)
        {
            if (!await Exists(name))
            {
                return null;
            }
            
            var value = _fs.File.ReadAllText(_fs.Path.Combine(_options.SystemFolderPath, "refs", name));
            if (string.IsNullOrEmpty(value))
            {
                return new EmptyReference();
            }

            var data = value.Split(new []{ ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 2)
            {
                throw new InvalidDataException($"Reference {name} not valid. Expected `prefix:value` format");
            }

            _logger?.Debug("Requested reference {ref}, value: {@value}", name, data);
            
            switch (data[0])
            {
                case "ref":
                    return new PointerReference(data[1]);
                
                case "commit":
                    return new CommitReference(data[1]);
                
                default:
                    throw new NotSupportedException($"Not supported `{data[0]}` reference type");
            }
        }
    }
}