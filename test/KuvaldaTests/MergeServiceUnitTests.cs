using System;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Kuvalda.Core.Merge;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class MergeServiceUnitTests
    {
        private MergeService _service;
        private Mock<ICommitGetService> _commitGetter;
        private Mock<IBaseCommitFinder> _baseFinder;
        private Mock<IDifferenceEntriesCreator> _entriesCreator;
        private Mock<ITreeMergeService> _treeMergeService;

        [SetUp]
        public void SetUp()
        {
            _commitGetter = new Mock<ICommitGetService>();
            _baseFinder = new Mock<IBaseCommitFinder>();
            _entriesCreator = new Mock<IDifferenceEntriesCreator>();
            _treeMergeService = new Mock<ITreeMergeService>();

            _service = new MergeService(_commitGetter.Object, _baseFinder.Object, _entriesCreator.Object, _treeMergeService.Object);
        }

        [Test]
        public void Test_Ctor_ShouldThrowIfCtorArgumentsNull()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentNullException>(() => new MergeService(null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new MergeService(_commitGetter.Object, null, null, null));
            Assert.Throws<ArgumentNullException>(() =>
                new MergeService(_commitGetter.Object, _baseFinder.Object, null, null));
            Assert.Throws<ArgumentNullException>(() =>
                new MergeService(_commitGetter.Object, _baseFinder.Object, _entriesCreator.Object, null));
        }

        [Test]
        public void Test_Merge_ShouldThrowIfArgumentsNullOrEmpty()
        {
            // Arrange/Act/Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.Merge(null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.Merge("", null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.Merge(" ", null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.Merge(" ", ""));
        }

        [Test]
        public async Task Test_Merge_ShouldReturnInconsistentTreesResult()
        {
            // Arrange
            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(typeof(MergeOperationInconsistentTreesResult), result.GetType());
        }

        [Test]
        public async Task Test_Merge_ShouldReturnModifiedConflictFile()
        {
            // Arrange
            var nowDatetime = DateTime.Now;

            var cmtBase = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("",
                    new TreeNodeFile("modified", nowDatetime.AddDays(-1), "a"))
            };

            _commitGetter.Setup(s => s.GetCommit(It.IsAny<string>())).Returns(Task.FromResult(cmtBase));
            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult("0"));
            _entriesCreator.Setup(s => s.Create(cmtBase.Tree, cmtBase.Tree))
                .Returns(new DifferenceEntries(new string[0], new[] {"modified"}, new string[0]));

            var expectedResult = new MergeOperationConflictResult()
            {
                ConflictedFiles = new[] {"modified"}
            };

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            var mergeConflict = result as MergeOperationConflictResult;
            Assert.NotNull(result);
            Assert.AreEqual(typeof(MergeOperationConflictResult), result.GetType());
            Assert.True(expectedResult.ConflictedFiles.SequenceEqual(mergeConflict.ConflictedFiles));
        }
        
        [Test]
        public async Task Test_Merge_ShouldReturnAddedAndRemovedConflictFile()
        {
            // Arrange
            var nowDatetime = DateTime.Now;

            var cmtBase = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("",
                    new TreeNodeFile("modified", nowDatetime.AddDays(-1), "a"))
            };

            _commitGetter.Setup(s => s.GetCommit(It.IsAny<string>())).Returns(Task.FromResult(cmtBase));
            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult("0"));
            _entriesCreator.SetupSequence(s => s.Create(cmtBase.Tree, cmtBase.Tree))
                .Returns(new DifferenceEntries(new[] {"modified"}, new string[0], new string[0]))
                .Returns(new DifferenceEntries(new string[0], new string[0], new[] {"modified"}));

            var expectedResult = new MergeOperationConflictResult()
            {
                ConflictedFiles = new[] {"modified"}
            };

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            var mergeConflict = result as MergeOperationConflictResult;
            Assert.NotNull(result);
            Assert.AreEqual(typeof(MergeOperationConflictResult), result.GetType());
            Assert.True(expectedResult.ConflictedFiles.SequenceEqual(mergeConflict.ConflictedFiles));
        }
        
        [Test]
        public async Task Test_Merge_ShouldReturnAddedDifferentConflictFile()
        {
            // Arrange
            var nowDatetime = DateTime.Now;

            var cmtBase = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("",
                    new TreeNodeFile("conflict", nowDatetime.AddDays(-1), "a"))
            };

            _commitGetter.Setup(s => s.GetCommit(It.IsAny<string>())).Returns(Task.FromResult(cmtBase));

            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult("0"));
            _entriesCreator.SetupSequence(s => s.Create(cmtBase.Tree, cmtBase.Tree))
                .Returns(new DifferenceEntries(new[] {"conflict"}, new string[0], new string[0]))
                .Returns(new DifferenceEntries(new[] {"conflict"}, new string[0], new string[0]));

            var expectedResult = new MergeOperationConflictResult()
            {
                ConflictedFiles = new[] {"conflict"}
            };

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            var mergeConflict = result as MergeOperationConflictResult;
            Assert.NotNull(result);
            Assert.AreEqual(typeof(MergeOperationConflictResult), result.GetType());
            Assert.True(expectedResult.ConflictedFiles.SequenceEqual(mergeConflict.ConflictedFiles));
        }

        [Test]
        public async Task Test_Merge_ShouldReturnMergeResult()
        {
            // Arrange
            var nowDatetime = DateTime.Now;

            var cmtBase = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("")
            };
            
            var left = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("",
                    new TreeNodeFile("fileLeft", nowDatetime.AddDays(-1), "a"))
            };
            
            var right = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("",
                    new TreeNodeFile("fileRight", nowDatetime.AddDays(-1), "b"))
            };

            _commitGetter.Setup(s => s.GetCommit("0")).Returns(Task.FromResult(cmtBase));
            _commitGetter.Setup(s => s.GetCommit("1")).Returns(Task.FromResult(left));
            _commitGetter.Setup(s => s.GetCommit("2")).Returns(Task.FromResult(right));

            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult("0"));
            _entriesCreator.Setup(s => s.Create(cmtBase.Tree, right.Tree))
                .Returns(new DifferenceEntries(new[] {"fileLeft"}, new string[0], new string[0]));
            _entriesCreator.Setup(s => s.Create(cmtBase.Tree, left.Tree))
                .Returns(new DifferenceEntries(new[] {"fileRight"}, new string[0], new string[0]));
            _treeMergeService.Setup(s => s.Merge(left.Tree, right.Tree)).Returns(new TreeNodeFolder(""));

            var expectedResult = new MergeOperationSuccessResult()
            {
                BaseCommit = "0",
                LeftParent = "1",
                RightParent = "2",
                MergedTree = new TreeNodeFolder("")
            };

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            var mergeConflict = result as MergeOperationSuccessResult;
            Assert.NotNull(result);
            Assert.AreEqual(typeof(MergeOperationSuccessResult), result.GetType());
            Assert.AreEqual(expectedResult.MergedTree, mergeConflict.MergedTree);
            Assert.AreEqual(expectedResult.BaseCommit, mergeConflict.BaseCommit);
            Assert.AreEqual(expectedResult.LeftParent, mergeConflict.LeftParent);
            Assert.AreEqual(expectedResult.RightParent, mergeConflict.RightParent);
        }
        
        [Test]
        public async Task Test_Merge_ShouldReturnMergeResultIgnoreFolderModifications()
        {
            // Arrange
            var nowDatetime = DateTime.Now;

            var cmtBase = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("")
            };
            
            var left = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("",
                    new TreeNodeFile("fileLeft", nowDatetime.AddDays(-1), "a"))
            };
            
            var right = new CommitDto()
            {
                Commit = new CommitModel() {Parents = new string[0]},
                Tree = new TreeNodeFolder("",
                    new TreeNodeFile("fileRight", nowDatetime.AddDays(-1), "b"))
            };

            _commitGetter.Setup(s => s.GetCommit("0")).Returns(Task.FromResult(cmtBase));
            _commitGetter.Setup(s => s.GetCommit("1")).Returns(Task.FromResult(left));
            _commitGetter.Setup(s => s.GetCommit("2")).Returns(Task.FromResult(right));

            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult("0"));
            _entriesCreator.Setup(s => s.Create(cmtBase.Tree, right.Tree))
                .Returns(new DifferenceEntries(new[] {"fileLeft"}, new string[0], new string[0]));
            _entriesCreator.Setup(s => s.Create(cmtBase.Tree, left.Tree))
                .Returns(new DifferenceEntries(new[] {"fileRight"}, new string[0], new string[0]));
            _treeMergeService.Setup(s => s.Merge(left.Tree, right.Tree)).Returns(new TreeNodeFolder(""));

            var expectedResult = new MergeOperationSuccessResult()
            {
                BaseCommit = "0",
                LeftParent = "1",
                RightParent = "2",
                MergedTree = new TreeNodeFolder("")
            };

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            var mergeConflict = result as MergeOperationSuccessResult;
            Assert.NotNull(result);
            Assert.AreEqual(typeof(MergeOperationSuccessResult), result.GetType());
            Assert.AreEqual(expectedResult.MergedTree, mergeConflict.MergedTree);
            Assert.AreEqual(expectedResult.BaseCommit, mergeConflict.BaseCommit);
            Assert.AreEqual(expectedResult.LeftParent, mergeConflict.LeftParent);
            Assert.AreEqual(expectedResult.RightParent, mergeConflict.RightParent);
        }
    }
}