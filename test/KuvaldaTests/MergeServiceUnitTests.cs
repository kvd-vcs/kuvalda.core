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

        [SetUp]
        public void SetUp()
        {
            _commitGetter = new Mock<ICommitGetService>();
            _baseFinder = new Mock<IBaseCommitFinder>();
            _entriesCreator = new Mock<IDifferenceEntriesCreator>();

            _service = new MergeService(_commitGetter.Object, _baseFinder.Object, _entriesCreator.Object);
        }

        [Test]
        public void Test_Ctor_ShouldThrowIfCtorArgumentsNull()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentNullException>(() => new MergeService(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new MergeService(_commitGetter.Object, null, null));
            Assert.Throws<ArgumentNullException>(() =>
                new MergeService(_commitGetter.Object, _baseFinder.Object, null));
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
    }
}