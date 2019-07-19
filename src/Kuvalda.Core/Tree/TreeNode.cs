using System;

namespace Kuvalda.Core
{
    public abstract class TreeNode : ICloneable
    {
        public string Name;

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

        public virtual bool DeepEquals(TreeNode other)
        {
            return Equals((object)other);
        }
    }
}