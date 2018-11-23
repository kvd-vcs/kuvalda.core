using System;
using Newtonsoft.Json;

namespace Kuvalda.Tree
{
    [JsonObject]
    public class TreeNodeFile : TreeNode
    {
        [JsonProperty(Order = 1)]
        public readonly DateTime ModificationTime;
        
        public TreeNodeFile(string name, DateTime modificationTime) : base(name)
        {
            ModificationTime = modificationTime;
        }

        protected bool Equals(TreeNodeFile other)
        {
            return base.Equals(other) && ModificationTime.Equals(other.ModificationTime);
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
            return new TreeNodeFile(Name, ModificationTime);
        }
    }
}