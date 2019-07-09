using System;
using Kuvalda.Core;
using Kuvalda.Core.Merge;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class TreeMergeServiceUnitTests
    {
        private TreeMergeService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new TreeMergeService();
        }

        [Test]
        public void Test_Merge_ShouldMerge2SimpleTrees()
        {
            // Arrange
            var dt = DateTime.Now;
            var leftTree = new TreeNodeFolder("", new TreeNodeFile("fileLeft", dt, "fileLeft"));
            var rightTree = new TreeNodeFolder("", new TreeNodeFile("fileRight", dt, "fileRight"));
            var expectedTree = new TreeNodeFolder("", new TreeNodeFile("fileLeft", dt, "fileLeft"),
                new TreeNodeFile("fileRight", dt, "fileRight"));

            // Act
            var result = _service.Merge(leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(expectedTree, result);
        }
    }
}