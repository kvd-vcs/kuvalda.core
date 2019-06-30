using System;
using System.IO;
using System.IO.Abstractions;
using Serilog;

namespace Kuvalda.Core
{
    public class RefsService : IRefsService
    {
        private readonly string _systemPath;
        private readonly IFileSystem _fs;
        private readonly RepositoryOptions _options;
        private readonly ILogger _logger;
        
        public RefsService(string systemPath, IFileSystem fileSystem, RepositoryOptions options, ILogger logger)
        {
            _systemPath = !string.IsNullOrEmpty(systemPath)
                ? systemPath
                : throw new ArgumentNullException(nameof(systemPath));
            _fs = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _options = options ?? throw new ArgumentNullException(nameof(options));;
            _logger = logger;
        }

        public bool Exists(string name)
        {
            return _fs.File.Exists(_fs.Path.Combine(_systemPath, "refs", name));
        }

        public Reference Get(string name)
        {
            var reference = GetInternal(name);
            
            if (reference is EmptyReference)
            {
                throw new InvalidDataException($"Reference {name} is empty");
            }

            return reference;
        }
        
        public string GetCommit(string name)
        {
            var reference = GetInternal(name);
            
            if (reference is EmptyReference)
            {
                throw new InvalidDataException($"Reference {name} is empty");
            }

            if (reference is CommitReference)
            {
                return reference.Value;
            }
            
            if (reference is PointerReference)
            {
                return GetCommit(reference.Value);
            }

            return reference.Value;
        }

        public void Store(string name, Reference value)
        {
            var prefix = value is PointerReference ? "ref" : "commit";
            var format = $"{prefix}:{value.Value}";
            _fs.File.WriteAllText(_fs.Path.Combine(_systemPath, "refs", name), format);
            _logger?.Debug("Stored reference {name} with data {data}", name, format);
        }

        public string GetHeadCommit()
        {
            var reference = GetHead();
            
            if (reference is PointerReference p)
            {
                var chashRef = Get(p.Value);
                if (chashRef == null)
                {
                    throw new InvalidDataException($"Reference is null. Check db consistency");
                }
                return chashRef.Value;
            }

            return reference.Value;
        }
        
        public void SetHead(Reference value)
        {
            var head = GetInternal(_options.HeadFilePath);
            
            switch (head)
            {
                case null:
                case EmptyReference _:
                    Store(_options.HeadFilePath, value);
                    break;
                case PointerReference _ when !(value is PointerReference):
                    Store(head.Value, value);
                    return;
            }

            Store(_options.HeadFilePath, value);
        }

        public Reference GetHead()
        {
            var reference = GetInternal(_fs.Path.Combine(_systemPath, "refs", _options.HeadFilePath));
            if (reference == null)
            {
                throw new FileNotFoundException($"Head reference not found. Check db consistency");
            }

            return reference;
        }
        
        private Reference GetInternal(string name)
        {
            if (!Exists(name))
            {
                return null;
            }
            
            var value = _fs.File.ReadAllText(_fs.Path.Combine(_systemPath, "refs", name));
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