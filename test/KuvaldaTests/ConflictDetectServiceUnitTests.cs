using System;
using System.Linq;
using Kuvalda.Core;
using Kuvalda.Core.Merge;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class ConflictDetectServiceUnitTests
    {
        private ConflictDetectService _service;
        private Mock<IDifferenceEntriesCreator> _diffCreator;
        private Mock<IFlatTreeCreator> _flatTreeCreator;

        [SetUp]
        public void SetUp()
        {
            _diffCreator = new Mock<IDifferenceEntriesCreator>();
            _flatTreeCreator = new Mock<IFlatTreeCreator>();
            _service = new ConflictDetectService(_diffCreator.Object, _flatTreeCreator.Object);
        }

        [Test]
        public void Test_Ctor_ThrowNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ConflictDetectService(null, null));
            Assert.Throws<ArgumentNullException>(() => new ConflictDetectService(_diffCreator.Object, null));
        }
        
        
        [Test]
        public void Test_Detect_ShouldReturnNoneConflicts()
        {
            // Arrange
            var baseTree = new TreeNodeFile("file", DateTime.Now, "file");
            var leftTree = new TreeNodeFile("file1", DateTime.Now, "file2");
            var rightTree = new TreeNodeFile("file2", DateTime.Now, "file2");

            _diffCreator.Setup(creator => creator.Create(baseTree, leftTree))
                .Returns(new DifferenceEntries(new[] {"file1"}, new string[0], new string[0]));
            _diffCreator.Setup(creator => creator.Create(baseTree, rightTree))
                .Returns(new DifferenceEntries(new[] {"file2"}, new string[0], new string[0]));

            // Act
            var result = _service.Detect(baseTree, leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        
        [Test]
        public void Test_Detect_ShouldReturnAddedConflictHashes()
        {
            // Arrange
            var now = DateTime.Now;
            var baseTree = new TreeNodeFile("file", now.AddDays(-1), "file");
            var leftTree = new TreeNodeFile("file1", now, "file2");
            var rightTree = new TreeNodeFile("file1", now.AddDays(1), "file1");
            var leftFlat = new[] {new FlatTreeItem("file1", leftTree)};
            var rightFlat = new[] {new FlatTreeItem("file1", rightTree)};
            
            _flatTreeCreator.Setup(s => s.Create(leftTree, "/")).Returns(leftFlat);
            _flatTreeCreator.Setup(s => s.Create(rightTree, "/")).Returns(rightFlat);

            _diffCreator.Setup(creator => creator.Create(baseTree, leftTree))
                .Returns(new DifferenceEntries(new[] {"file1"}, new string[0], new string[0]));
            _diffCreator.Setup(creator => creator.Create(baseTree, rightTree))
                .Returns(new DifferenceEntries(new[] {"file1"}, new string[0], new string[0]));

            var expected = new[]
            {
                new MergeConflict("file1", MergeConflictReason.Added, MergeConflictReason.Added)
            };

            // Act
            var result = _service.Detect(baseTree, leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.SequenceEqual(expected));
        }
        
        [Test]
        public void Test_Detect_ShouldReturnAddedDifferentTimeButSomeHash()
        {
            // Arrange
            var baseTree = new TreeNodeFile("file", DateTime.Now, "file");
            var leftTree = new TreeNodeFile("file1", DateTime.Now, "file");
            var rightTree = new TreeNodeFile("file1", DateTime.Now, "file");
            var leftFlat = new[] {new FlatTreeItem("file1", leftTree)};
            var rightFlat = new[] {new FlatTreeItem("file1", rightTree)};

            _diffCreator.Setup(creator => creator.Create(baseTree, leftTree))
                .Returns(new DifferenceEntries(new[] {"file1"}, new string[0], new string[0]));
            _diffCreator.Setup(creator => creator.Create(baseTree, rightTree))
                .Returns(new DifferenceEntries(new[] {"file1"}, new string[0], new string[0]));

            _flatTreeCreator.Setup(s => s.Create(leftTree, "/")).Returns(leftFlat);
            _flatTreeCreator.Setup(s => s.Create(rightTree, "/")).Returns(rightFlat);

            var expected = new MergeConflict[0];

            // Act
            var result = _service.Detect(baseTree, leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.SequenceEqual(expected));
        }
        
        [Test]
        public void Test_Detect_ShouldReturnModifiedSomeFilesConflict()
        {
            // Arrange
            var now = DateTime.Now;
            var baseTree = new TreeNodeFile("file", now, "file");
            var leftTree = new TreeNodeFile("file", now.AddDays(1), "file1");
            var rightTree = new TreeNodeFile("file", now.AddDays(2), "file2");
            var leftFlat = new[] {new FlatTreeItem("file", leftTree)};
            var rightFlat = new[] {new FlatTreeItem("file", rightTree)};

            _diffCreator.Setup(creator => creator.Create(baseTree, leftTree))
                .Returns(new DifferenceEntries(new string[0], new []{"file"}, new string[0]));
            _diffCreator.Setup(creator => creator.Create(baseTree, rightTree))
                .Returns(new DifferenceEntries(new string[0], new []{"file"}, new string[0]));

            _flatTreeCreator.Setup(s => s.Create(leftTree, "/")).Returns(leftFlat);
            _flatTreeCreator.Setup(s => s.Create(rightTree, "/")).Returns(rightFlat);

            var expected = new[]
            {
                new MergeConflict("file", MergeConflictReason.Modify, MergeConflictReason.Modify),
            };

            // Act
            var result = _service.Detect(baseTree, leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.SequenceEqual(expected));
        }
        
        [Test]
        public void Test_Detect_ShouldReturnOkIfDifferentTimeAndSomeHashes()
        {
            // Arrange
            var now = DateTime.Now;
            var baseTree = new TreeNodeFile("file", now, "file");
            var leftTree = new TreeNodeFile("file", now.AddDays(1), "file1");
            var rightTree = new TreeNodeFile("file", now.AddDays(2), "file1");
            var leftFlat = new[] {new FlatTreeItem("file", leftTree)};
            var rightFlat = new[] {new FlatTreeItem("file", rightTree)};

            _diffCreator.Setup(creator => creator.Create(baseTree, leftTree))
                .Returns(new DifferenceEntries(new string[0], new []{"file"}, new string[0]));
            _diffCreator.Setup(creator => creator.Create(baseTree, rightTree))
                .Returns(new DifferenceEntries(new string[0], new []{"file"}, new string[0]));

            _flatTreeCreator.Setup(s => s.Create(leftTree, "/")).Returns(leftFlat);
            _flatTreeCreator.Setup(s => s.Create(rightTree, "/")).Returns(rightFlat);

            var expected = new MergeConflict[0];

            // Act
            var result = _service.Detect(baseTree, leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.SequenceEqual(expected));
        }
        
        [Test]
        public void Test_Detect_ShouldReturnConflictForAddedFileAndFolderWithSomeName()
        {
            // Arrange
            var now = DateTime.Now;
            var baseTree = new TreeNodeFile("file", now, "file");
            var leftTree = new TreeNodeFile("conflict", now.AddDays(1), "conflict");
            var rightTree = new TreeNodeFolder("conflict");
            var leftFlat = new[] {new FlatTreeItem("conflict", leftTree)};
            var rightFlat = new[] {new FlatTreeItem("conflict", rightTree)};

            _diffCreator.Setup(creator => creator.Create(baseTree, leftTree))
                .Returns(new DifferenceEntries(new []{"conflict"}, new string[0], new string[0]));
            _diffCreator.Setup(creator => creator.Create(baseTree, rightTree))
                .Returns(new DifferenceEntries(new []{"conflict"}, new string[0], new string[0]));

            _flatTreeCreator.Setup(s => s.Create(leftTree, "/")).Returns(leftFlat);
            _flatTreeCreator.Setup(s => s.Create(rightTree, "/")).Returns(rightFlat);

            var expected = new[]
            {
                new MergeConflict("conflict", MergeConflictReason.Added, MergeConflictReason.Added)
            };

            // Act
            var result = _service.Detect(baseTree, leftTree, rightTree);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.SequenceEqual(expected));
        }
    }
}