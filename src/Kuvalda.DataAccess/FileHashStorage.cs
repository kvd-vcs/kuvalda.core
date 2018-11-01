using Kuvalda.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvalda.DataAccess
{
    public class FileHashStorage
    {
        public readonly string StorageFolder;


        public FileHashStorage(string storageFolder)
        {
            StorageFolder = storageFolder ?? throw new ArgumentNullException(nameof(storageFolder));

            if(!Directory.Exists(StorageFolder))
            {
                throw new DirectoryNotFoundException($"Storage folder not found at file system in path: {storageFolder}");
            }
        }


        public Stream Get(Hash address)
        {
            if(address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!HasBlob(address))
            {
                throw new FileNotFoundException("Blon not found in hash file storage", address.Value);
            }

            return File.OpenRead(Path.Combine(StorageFolder, address.Value));
        }

        public async Task<Hash> Set(Stream blob)
        {
            if (blob == null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            var seek = blob.Position;
            var hash = blob.GetSHA1();
            blob.Seek(seek, SeekOrigin.Begin);

            var pathToFile = Path.Combine(StorageFolder, hash.Value);

            using (var file = new FileStream(pathToFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await blob.CopyToAsync(file);
                await file.FlushAsync();
                file.Close();
            }

            return hash;
        }

        public bool HasBlob(Hash address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var pathToFile = Path.Combine(StorageFolder, address.Value);

            if (File.Exists(pathToFile))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Hash> GetAllBlobsAddresses()
        {
            return Directory.GetFiles(StorageFolder).Select(entry => new SHA1Hash(entry));
        }
    }
}
