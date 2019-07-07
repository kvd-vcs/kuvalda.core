using System.Collections.Generic;
using System.Linq;

namespace Kuvalda.Core
{
    public class TreeNodeFolder : TreeNode
    {
        public IEnumerable<TreeNode> Nodes;
        
        public TreeNodeFolder(string name) : base(name)
        {
            Nodes = new TreeNode[0];
        }
        
        public TreeNodeFolder(string name, params TreeNode[] childs) : base(name)
        {
            Nodes = childs;
        }
        
        protected bool Equals(TreeNodeFolder other)
        {
            return base.Equals(other) && Nodes.SequenceEqual(other.Nodes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TreeNodeFolder) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Nodes != null ? Nodes.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override object Clone()
        {
            return new TreeNodeFolder(Name)
            {
                Nodes = Nodes?.Select(node => (TreeNode) node.Clone())
            };
        }
    }
}