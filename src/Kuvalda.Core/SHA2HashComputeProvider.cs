using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kuvalda.Core
{
    public class SHA2HashComputeProvider : IHashComputeProvider
    {
        public async Task<string> Compute(Stream stream)
        {
            string hashString;
            using (var algorithm = new SHA256Managed())
            {
                var hash = await Task.Run(() => algorithm.ComputeHash(stream));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                hashString = sb.ToString();
            }

            return hashString;
        }
    }
}