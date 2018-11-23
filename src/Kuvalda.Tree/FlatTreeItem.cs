namespace Kuvalda.Tree
{
    public class FlatTreeItem
    {
        public string Name;
        public TreeNode Node;

        public FlatTreeItem(string name, TreeNode node)
        {
            Name = name;
            Node = node;
        }
    }
}