using System;
using System.Globalization;

namespace Kuvalda.Core
{
    public class TreeNodeFile : TreeNode
    {
        public DateTime ModificationTime;
        public string Hash;
        
        public TreeNodeFile(string name, DateTime modificationTime, string hash = null) : base(name)
        {
            ModificationTime = modificationTime;
            Hash = hash;
        }

        protected bool Equals(TreeNodeFile other)
        {
            return base.Equals(other) && Math.Abs((ModificationTime - other.ModificationTime).TotalSeconds) < 0.5f;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TreeNodeFile) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ ModificationTime.GetHashCode();
                return hashCode;
            }
        }

        public override object Clone()
        {
            return new TreeNodeFile(Name, ModificationTime, Hash);
        }

        public override bool DeepEquals(TreeNode other)
        {
            return Equals((object)other) && string.Equals(Hash, ((TreeNodeFile)other).Hash);
        }

        public override string ToString()
        {
            return $"Type: file, Name: {Name}, ModificationTime: {ModificationTime.ToString(CultureInfo.InvariantCulture)}, Hash: {Hash}";
        }
    }
}