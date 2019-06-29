using System;
using System.Threading.Tasks;
using Kuvalda.Core;
using Newtonsoft.Json;

namespace Kuvalda.Cli
{
    public class CompressCommand : ICliCommand
    {
        private readonly IChangesCompressService _changesCompressService;
        private readonly IEntityObjectStorage<CompressModel> _storage;

        public CompressCommand(IChangesCompressService changesCompressService, IEntityObjectStorage<CompressModel> storage)
        {
            _changesCompressService = changesCompressService;
            _storage = storage;
        }

        public async Task<int> Execute(string[] args)
        {
            var compressed = await _changesCompressService.Compress(args[1], args[2]);
            var hash = await _storage.Store(compressed);

            Console.WriteLine(JsonConvert.SerializeObject(compressed, Formatting.Indented));
            Console.WriteLine(hash);
            
            return 0;
        }
    }
    
    public class DecompressCommand : ICliCommand
    {
        private readonly IChangesDecompressService _changesCompressService;
        private readonly IEntityObjectStorage<CompressModel> _storage;

        public DecompressCommand(IChangesDecompressService changesCompressService, IEntityObjectStorage<CompressModel> storage)
        {
            _changesCompressService = changesCompressService;
            _storage = storage;
        }

        public async Task<int> Execute(string[] args)
        {
            var compress = await _storage.Get(args[1]);
            await _changesCompressService.Apply(compress, Environment.CurrentDirectory);
            
            Console.WriteLine(JsonConvert.SerializeObject(compress, Formatting.Indented));

            return 0;
        }
    }
}