using System;
using System.Linq;
using Kuvalda.Core;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class FlatTreeDifferTest
    {
        [Test]
        public void Test_ShouldExcept()
        {
            // Arrange
            var differ = new FlatTreeDiffer();

            var flatTreeLeft = new[]
            {
                new FlatTreeItem("nomodify", new TreeNodeFile("nomodify", new DateTime(0))),
                new FlatTreeItem("modify", new TreeNodeFile("modify", new DateTime(1))),
                new FlatTreeItem("removed", new TreeNodeFile("removed", new DateTime(1))),
            };
            
            var flatTreeRight = new[]
            {
                new FlatTreeItem("nomodify", new TreeNodeFile("nomodify", new DateTime(0))),
                new FlatTreeItem("modify", new TreeNodeFile("modify", new DateTime(2))),
                new FlatTreeItem("added", new TreeNodeFile("added", new DateTime(1))),
            };
            
            // Act
            var added = differ.Except(flatTreeRight, flatTreeLeft);
            
            // Assert
            Assert.AreEqual(added.Single(), flatTreeRight[2]);
        }
        
        [Test]
        public void Test_ShouldIntersect()
        {
            // Arrange
            var differ = new FlatTreeDiffer();

            var flatTreeLeft = new[]
            {
                new FlatTreeItem("nomodify", new TreeNodeFile("nomodify", new DateTime(0))),
                new FlatTreeItem("modify", new TreeNodeFile("modify", new DateTime(1))),
                new FlatTreeItem("removed", new TreeNodeFile("removed", new DateTime(1))),
            };
            
            var flatTreeRight = new[]
            {
                new FlatTreeItem("nomodify", new TreeNodeFile("nomodify", new DateTime(0))),
                new FlatTreeItem("modify", new TreeNodeFile("modify", new DateTime(2))),
                new FlatTreeItem("added", new TreeNodeFile("added", new DateTime(1))),
            };
            
            // Act
            var added = differ.Intersect(flatTreeRight, flatTreeLeft);
            
            // Assert
            Assert.AreEqual(added.First(), flatTreeRight[0]);
            Assert.AreEqual(added.Last(), flatTreeRight[1]);
        }
        
        [Test]
        public void Test_ShouldReturnDiff()
        {
            // Arrange
            var differ = new FlatTreeDiffer();

            var flatTreeLeft = new[]
            {
                new FlatTreeItem("nomodify", new TreeNodeFile("nomodify", new DateTime(0))),
                new FlatTreeItem("modify", new TreeNodeFile("modify", new DateTime(1))),
                new FlatTreeItem("removed", new TreeNodeFile("removed", new DateTime(1))),
            };
            
            var flatTreeRight = new[]
            {
                new FlatTreeItem("nomodify", new TreeNodeFile("nomodify", new DateTime(0))),
                new FlatTreeItem("modify", new TreeNodeFile("modify", new DateTime(2))),
                new FlatTreeItem("added", new TreeNodeFile("added", new DateTime(1))),
            };
            
            // Act
            var added = differ.Difference(flatTreeLeft, flatTreeRight);
            
            // Assert
            Assert.AreEqual(added.Single(), flatTreeRight[1]);
        }

    }
}