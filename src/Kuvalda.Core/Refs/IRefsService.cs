using System;
using System.IO;
using System.IO.Abstractions;

namespace Kuvalda.Core
{
    public abstract class Reference
    {
        public string Value { get; set; }
    }
    
    public class PointerReference : Reference
    {
        public PointerReference(string refName)
        {
            Value = refName;
        }
    }
    
    public class CommitReference : Reference
    {
        public CommitReference(string chash)
        {
            Value = chash;
        }
    }
    
    public interface IRefsService
    {
        bool Exists(string name);
        Reference Get(string name);
        void Store(string name, Reference reference);
        Reference GetHead();
        string GetHeadCommit();
        void SetHead(Reference value);
    }

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
            if (!Exists(name))
            {
                return null;
            }
            
            var value = FileSyste.File.ReadAllText(FileSyste.Path.Combine(SystemPath, "refs", name));
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidDataException($"Reference {name} is empty");
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

        public void Store(string name, Reference value)
        {
            var prefix = value is PointerReference ? "ref" : "commit";
            var format = $"{prefix}:{value}";
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
            Store(Options.HeadFilePath, value);
        }

        public Reference GetHead()
        {
            var reference = Get(FileSyste.Path.Combine(SystemPath, "refs", Options.HeadFilePath));
            if (reference == null)
            {
                throw new FileNotFoundException($"Head reference not found. Check db consistency");
            }

            return reference;
        }
    }
}