using System;
using System.Threading.Tasks;
using Kuvalda.Core;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class BaseCommitFinderUnitTests
    {
        private BaseCommitFinder _service;
        
        private Mock<IEntityObjectStorage<CommitModel>> _storage;

        [SetUp]
        public void SetUp()
        {
            _storage = new Mock<IEntityObjectStorage<CommitModel>>();
            _service = new BaseCommitFinder(_storage.Object);
        }
        
        [Test]
        public void Test_Ctor_ShouldThrowIfCtorArgumentsNull()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentNullException>(() => new BaseCommitFinder(null));
        }
        
                
        [Test]
        public void Test_FindBase_ShouldThrowIfParametersNullOrEmpty()
        {
            // Arrange/Act/Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.FindBase(null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.FindBase("", null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.FindBase(" ", null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.FindBase(" ", ""));
        }
        
        [Test]
        public async Task Test_FindBase_ShouldReturnNullIfBaseNotExists()
        {
            // Arrange
            _storage.Setup(s => s.Get("1"))
                .Returns(Task.FromResult(new CommitModel {Parents = new string[0]}));
            
            _storage.Setup(s => s.Get("2"))
                .Returns(Task.FromResult(new CommitModel {Parents = new string[0]}));
            
            // Act
            var result = await _service.FindBase("1", "2");
            
            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task Test_FindBase_ShouldReturnBaseCommitForSingleBinaryTree()
        {
            // Arrange
            _storage.Setup(s => s.Get("0"))
                .Returns(Task.FromResult(new CommitModel {Parents = new string[0]}));
            
            _storage.Setup(s => s.Get("1"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"0"}}));
            
            _storage.Setup(s => s.Get("2"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"0"}}));
            
            // Act
            var result = await _service.FindBase("1", "2");
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("0", result);
        }
        
        [Test]
        public async Task Test_FindBase_ShouldReturnSomeCommitIfLeftEqualRight()
        {
            // Arrange
            var commit = new CommitModel {Parents = new string[0]};
            _storage.Setup(s => s.Get("0")).Returns(Task.FromResult(commit));

            // Act
            var result = await _service.FindBase("0", "0");
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("0", result);
        }
        
        [Test]
        public async Task Test_FindBase_ShouldReturnBaseCommitForSingleBinaryTreeWithTail()
        {
            // Arrange
            _storage.Setup(s => s.Get("0"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"-1"}}));
            
            _storage.Setup(s => s.Get("1"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"0"}}));
            
            _storage.Setup(s => s.Get("2"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"0"}}));
            
            // Act
            var result = await _service.FindBase("1", "2");
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("0", result);
        }
        
        [Test]
        public async Task Test_FindBase_ShouldReturnBaseCommitForTreeWithMerges()
        {
            // Arrange
            _storage.Setup(s => s.Get("-1"))
                .Returns(Task.FromResult(new CommitModel {Parents = new string[0]}));
            
            _storage.Setup(s => s.Get("0"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"-1"}}));
            
            _storage.Setup(s => s.Get("1"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"-1", "0"}}));
            
            _storage.Setup(s => s.Get("2"))
                .Returns(Task.FromResult(new CommitModel {Parents = new []{"0"}}));
            
            // Act
            var result = await _service.FindBase("1", "2");
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("0", result);
        }
    }
}