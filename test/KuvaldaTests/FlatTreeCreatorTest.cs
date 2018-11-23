using System;
using System.Linq;
using Kuvalda.Tree;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class FlatTreeCreatorTest
    {
        [Test]
        public void Test_ShouldConstruct()
        {
            new FlatTreeCreator();
        }

        [Test]
        public void Test_ShouldFlatTree()
        {
            // Arrange
            var flatter = new FlatTreeCreator();
            var tree = new TreeNodeFolder("")
            {
                Nodes = new TreeNode[]
                {
                    new TreeNodeFile("file", new DateTime()),
                    new TreeNodeFolder("folder")
                    {
                        Nodes = new []
                        {
                            new TreeNodeFile("file1", new DateTime()),
                            new TreeNodeFile("file2", new DateTime())
                        }
                    }
                }
            };
            
            // Act
            var flat = flatter.Create(tree).ToList();
            
            // Assetr
            Assert.AreEqual(5, flat.Count());
            
            Assert.AreEqual(flat[0].Name, "/");
            Assert.AreEqual(flat[0].Node, tree);
            
            Assert.AreEqual(flat[1].Name, "/" + tree.Nodes.ElementAt(0).Name);
            Assert.AreEqual(flat[1].Node, tree.Nodes.ElementAt(0));
            
            Assert.AreEqual(flat[2].Name, "/" + tree.Nodes.ElementAt(1).Name);
            Assert.AreEqual(flat[2].Node, tree.Nodes.ElementAt(1));
            
            Assert.AreEqual(flat[3].Name, "/" + tree.Nodes.ElementAt(1).Name + "/" + ((TreeNodeFolder)tree.Nodes.ElementAt(1)).Nodes.ElementAt(0).Name);
            Assert.AreEqual(flat[3].Node, ((TreeNodeFolder)tree.Nodes.ElementAt(1)).Nodes.ElementAt(0));
            
            Assert.AreEqual(flat[4].Name, "/" + tree.Nodes.ElementAt(1).Name + "/" + ((TreeNodeFolder)tree.Nodes.ElementAt(1)).Nodes.ElementAt(1).Name);
            Assert.AreEqual(flat[4].Node, ((TreeNodeFolder)tree.Nodes.ElementAt(1)).Nodes.ElementAt(1));
        }
        
        [Test]
        public void Test_ShouldThrowArgumentNull()
        {
            // Arrange
            var flatter = new FlatTreeCreator();
            
            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => flatter.Create(null));
            
        }

    }
}
