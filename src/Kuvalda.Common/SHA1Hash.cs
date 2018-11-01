using System;

namespace Kuvalda.Common
{
    public class SHA1Hash : Hash
    {
        public SHA1Hash(string hash) : base(hash)
        { }

        public override string Validate(string hash)
        {
            if(hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            hash = hash.ToLower();

            if(hash.StartsWith("sha1:"))
            {
                hash = hash.Substring(5);
            }

            if(hash.Length != 40)
            {
                throw new FormatException("Hash length not equal 40 digits");
            }
            
            for(int i = 0; i < hash.Length; i++)
            {
                var charAtI = hash[i];
                if (char.IsLetterOrDigit(charAtI))
                {
                    continue;
                }

                throw new FormatException($"Hash contain not digit and letter character at {i} position");
            }

            return hash;
        }
    }
}
