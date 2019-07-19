using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Kuvalda.Core.Merge;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class MergeServiceUnitTests
    {
        private MergeService _service;
        private Mock<ICommitGetService> _commitGetter;
        private Mock<IBaseCommitFinder> _baseFinder;
        private Mock<ITreeMergeService> _treeMergeService;
        private Mock<IConflictDetectService> _conflictDetecter;

        [SetUp]
        public void SetUp()
        {
            _commitGetter = new Mock<ICommitGetService>();
            _baseFinder = new Mock<IBaseCommitFinder>();
            _treeMergeService = new Mock<ITreeMergeService>();
            _conflictDetecter = new Mock<IConflictDetectService>();

            _service = new MergeService(_commitGetter.Object, _baseFinder.Object, _treeMergeService.Object, _conflictDetecter.Object);
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
                new MergeService(_commitGetter.Object, _baseFinder.Object,  _treeMergeService.Object, null));
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
        public async Task Test_Merge_ShouldReturnConflict()
        {
            // Arrange
            var cmtBase = new CommitDto()
            {
                Tree = new TreeNodeFolder("")
            };
            var cmtLeft = new CommitDto()
            {
                Tree = new TreeNodeFolder("left")
            };
            var cmtRight = new CommitDto()
            {
                Tree = new TreeNodeFolder("right")
            };

            var expectedResult = new MergeOperationConflictResult()
            {
                ConflictedFiles = new[]
                    {new MergeConflict("conflict", MergeConflictReason.Added, MergeConflictReason.Added),}
            };
            
            _commitGetter.Setup(s => s.GetCommit("0")).Returns(Task.FromResult(cmtBase));
            _commitGetter.Setup(s => s.GetCommit("1")).Returns(Task.FromResult(cmtLeft));
            _commitGetter.Setup(s => s.GetCommit("2")).Returns(Task.FromResult(cmtRight));
            
            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult("0"));
            _conflictDetecter.Setup(s => s.Detect(cmtBase.Tree, cmtLeft.Tree, cmtRight.Tree)).Returns(expectedResult.ConflictedFiles);

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public async Task Test_Merge_ShouldReturnMergeResult()
        {
            // Arrange
            var expectedResult = new MergeOperationSuccessResult()
            {
                BaseCommit = "0",
                LeftParent = "1",
                RightParent = "2",
                MergedTree = new TreeNodeFolder("")
            };
            
            var cmtBase = new CommitDto()
            {
                Tree = new TreeNodeFolder("")
            };
            var cmtLeft = new CommitDto()
            {
                Tree = new TreeNodeFolder("left")
            };
            var cmtRight = new CommitDto()
            {
                Tree = new TreeNodeFolder("right")
            };

            _commitGetter.Setup(s => s.GetCommit("0")).Returns(Task.FromResult(cmtBase));
            _commitGetter.Setup(s => s.GetCommit("1")).Returns(Task.FromResult(cmtLeft));
            _commitGetter.Setup(s => s.GetCommit("2")).Returns(Task.FromResult(cmtRight));
            
            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult("0"));
            _conflictDetecter.Setup(s => s.Detect(cmtBase.Tree, cmtLeft.Tree, cmtRight.Tree)).Returns(new MergeConflict[0]);
            _treeMergeService.Setup(s => s.Merge(cmtLeft.Tree, cmtRight.Tree)).Returns(expectedResult.MergedTree);

            // Act
            var result = await _service.Merge("1", "2");

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(expectedResult, result);
        }
    }
}