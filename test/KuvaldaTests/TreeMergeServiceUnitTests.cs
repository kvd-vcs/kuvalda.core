using System;
using Kuvalda.Core;
using Kuvalda.Core.Exceptions;
using Kuvalda.Core.Merge;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class TreeMergeServiceUnitTests
    {
        private TreeMergeService _service;
        private Mock<INowDateTimeService> _nowDateTimeService;
        
        [SetUp]
        public void SetUp()
        {
            _nowDateTimeService = new Mock<INowDateTimeService>();
            _service = new TreeMergeService(_nowDateTimeService.Object);
        }

        [Test]
        public void Test_Merge_ShouldMergeThrowNullArguments()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentNullException>(() => _service.Merge(null, null));
            Assert.Throws<ArgumentNullException>(() => _service.Merge(new TreeNodeFolder(""), null));
        }
        
        [Test]
        public void Test_Merge_ShouldThrowIfConflictHashes()
        {
            // Arrange
            var dt = DateTime.Now;
            var leftTree = new TreeNodeFile("file", dt, "1");
            var rightTree = new TreeNodeFile("file", dt, "2");

            // Act/Assert
            Assert.Throws<ConflictTreeException>(() => _service.Merge(leftTree, rightTree));
        }
        
        [Test]
        public void Test_Merge_ShouldThrowIfConflictHashesAndTime()
        {
            // Arrange
            var dt = DateTime.Now;
            var leftTree = new TreeNodeFile("file", dt.AddMinutes(10), "1");
            var rightTree = new TreeNodeFile("file", dt, "2");

            // Act/Assert
            Assert.Throws<ConflictTreeException>(() => _service.Merge(leftTree, rightTree));
        }
        
        [Test]
        public void Test_Merge_ShouldThrowIfConflictHashesAndTimeInFolder()
        {
            // Arrange
            var dt = DateTime.Now;
            var leftTree = new TreeNodeFolder("", new TreeNodeFile("file", dt.AddMinutes(10), "1"));
            var rightTree =  new TreeNodeFolder("", new TreeNodeFile("file", dt, "2"));

            // Act/Assert
            Assert.Throws<ConflictTreeException>(() => _service.Merge(leftTree, rightTree));
        }
        
        [Test]
        public void Test_Merge_ShouldThrowIfFolderAndFile()
        {
            // Arrange
            var dt = DateTime.Now;
            var leftTree = new TreeNodeFolder("", new TreeNodeFile("file", dt, "1"));
            var rightTree =  new TreeNodeFolder("", new TreeNodeFolder("file"));

            // Act/Assert
            Assert.Throws<ConflictTreeException>(() => _service.Merge(leftTree, rightTree));
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
        
        [Test]
        public void Test_Merge_ShouldMergeAddedFile()
        {
            // Arrange
            var dt = DateTime.Now;
            var leftTree = new TreeNodeFolder("");
            var rightTree = new TreeNodeFolder("", new TreeNodeFile("fileRight", dt, "fileRight"));

            // Act
            var result = _service.Merge(leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(rightTree, result);
        }
        
        [Test]
        public void Test_Merge_ShouldMergeDifferentTimestamp()
        {
            // Arrange
            var dt = DateTime.Now;

            _nowDateTimeService.Setup(c => c.GetNow()).Returns(dt);
            
            var leftTree = new TreeNodeFile("file", dt.AddDays(-1), "file");
            var rightTree = new TreeNodeFile("file", dt.AddDays(-2), "file");
            var expectedTree =  new TreeNodeFile("file", dt, "file");

            // Act
            var result = _service.Merge(leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(expectedTree, result);
        }
    }
}