using System;

namespace Kuvalda.Core
{
    public class TreeNodeFile : TreeNode
    {
        public readonly DateTime ModificationTime;
        public string Hash;
        
        public TreeNodeFile(string name, DateTime modificationTime, string hash = null) : base(name)
        {
            ModificationTime = modificationTime;
            Hash = hash;
        }

        protected bool Equals(TreeNodeFile other)
        {
            return base.Equals(other) && (ModificationTime - other.ModificationTime).TotalSeconds < 0.5f;
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
    }
}