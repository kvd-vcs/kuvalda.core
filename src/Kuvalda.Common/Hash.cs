namespace Kuvalda.Common
{
    public abstract class Hash
    {
        public readonly string Value;

        public Hash(string hash)
        {
            Value = Validate(hash);
        }

        public abstract string Validate(string hash);

        public string Take(int start, int count)
        {
            return Value.Substring(start, count);
        }
    }
}