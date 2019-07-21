using System;
using System.IO;
using Serilog;

namespace Kuvalda.Core
{
    public class ReferenceFactory : IReferenceFactory
    {
        private readonly ILogger _logger;

        public ReferenceFactory(ILogger logger)
        {
            _logger = logger;
        }

        public Reference Create(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new EmptyReference();
            }

            var data = value.Split(new []{ ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 2)
            {
                throw new InvalidDataException($"Reference value {value} not valid. Expected `prefix:value` format");
            }

            _logger?.Debug("Requested reference value: {@value}", data);
            
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