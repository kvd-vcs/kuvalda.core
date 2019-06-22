using System;
using System.IO;
using System.IO.Abstractions;

namespace Kuvalda.Core
{
    public class RefsService : IRefsService
    {
        private readonly string SystemPath;
        private readonly IFileSystem FileSyste;
        private readonly RepositoryOptions Options;
        
        public RefsService(string systemPath, IFileSystem fileSyste, RepositoryOptions options)
        {
            SystemPath = systemPath;
            FileSyste = fileSyste;
            Options = options;
        }

        public bool Exists(string name)
        {
            return FileSyste.File.Exists(FileSyste.Path.Combine(SystemPath, "refs", name));
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

        public void Store(string name, Reference value)
        {
            var prefix = value is PointerReference ? "ref" : "commit";
            var format = $"{prefix}:{value.Value}";
            FileSyste.File.WriteAllText(FileSyste.Path.Combine(SystemPath, "refs", name), format);
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
            var head = GetInternal(Options.HeadFilePath);
            
            switch (head)
            {
                case null:
                case EmptyReference _:
                    Store(Options.HeadFilePath, value);
                    break;
                case PointerReference _ when !(value is PointerReference):
                    Store(head.Value, value);
                    return;
            }

            Store(Options.HeadFilePath, value);
        }

        public Reference GetHead()
        {
            var reference = GetInternal(FileSyste.Path.Combine(SystemPath, "refs", Options.HeadFilePath));
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
            
            var value = FileSyste.File.ReadAllText(FileSyste.Path.Combine(SystemPath, "refs", name));
            if (string.IsNullOrEmpty(value))
            {
                return new EmptyReference();
            }

            var data = value.Split(new []{ ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 2)
            {
                throw new InvalidDataException($"Reference {name} not valid. Expected `prefix:value` format");
            }

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