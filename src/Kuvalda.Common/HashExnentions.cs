using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kuvalda.Common
{
    public static class HashExnentions
    {
        public static Hash GetSHA1(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (BufferedStream bs = new BufferedStream(stream))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return new SHA1Hash(formatted.ToString());
                }
            }
        }
    }
}
