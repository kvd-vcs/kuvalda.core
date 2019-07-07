using System;
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
        
        [SetUp]
        public void SetUp()
        {
            _commitGetter = new Mock<ICommitGetService>();
            _baseFinder = new Mock<IBaseCommitFinder>();
            
            _service = new MergeService(_commitGetter.Object, _baseFinder.Object);
        }

        [Test]
        public void Test_Ctor_ShouldThrowIfCtorArgumentsNull()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentNullException>(() => new MergeService(null, null));
            Assert.Throws<ArgumentNullException>(() => new MergeService(_commitGetter.Object, null));
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
            _baseFinder.Setup(c => c.FindBase("1", "2")).Returns(Task.FromResult<CommitModel>(null));
            
            // Act
            var result = await _service.Merge("1", "2");
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(typeof(MergeOperationInconsistentTreesResult), result.GetType());
        }
    }
}