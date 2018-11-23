using System;
using Newtonsoft.Json;

namespace Kuvalda.Tree
{
    [JsonObject]
    public abstract class TreeNode : ICloneable
    {
        [JsonProperty(Order = 0)]
        public readonly string Name;

        protected TreeNode(string name)
        {
            Name = name;
        }
        
        protected bool Equals(TreeNode other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TreeNode) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public abstract object Clone();
    }
}