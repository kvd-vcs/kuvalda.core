using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Kuvalda.Core;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class CommitStoreServiceTests
    {

        private CommitStoreService _commitStoreService;
        
        private Mock<IEntityObjectStorage<CommitModel>> _commitStorageMock;
        private Mock<IEntityObjectStorage<TreeNode>> _treeStorageMock;
        private MockFileSystem _fileSystem;
        private Mock<IObjectStorage> _blobStorageMock;
        private Mock<IFlatTreeCreator> _flatTreeCreator;

        [SetUp]
        public void SetUp()
        {
            _commitStorageMock = new Mock<IEntityObjectStorage<CommitModel>>();
            _treeStorageMock = new Mock<IEntityObjectStorage<TreeNode>>();
            _fileSystem = new MockFileSystem();
            _blobStorageMock = new Mock<IObjectStorage>();
            _flatTreeCreator = new Mock<IFlatTreeCreator>();

            _commitStoreService = new CommitStoreService(_commitStorageMock.Object, _treeStorageMock.Object,
                _fileSystem, _blobStorageMock.Object, _flatTreeCreator.Object);
        }

        [Test]
        public async Task Test_Store_ShouldStoreOneBlob()
        {
            // Arrane
            var chash = "ca39a3ee5e6b4b0d3255bfef95601890afd80709";
            var thash = "ta39a3ee5e6b4b0d3255bfef95601890afd80709";
            var fhash = "fa39a3ee5e6b4b0d3255bfef95601890afd80709";
            
            var commitModel = new CommitModel()
            {
                Labels = new Dictionary<string, string>(),
            };
            var node = (TreeNode)new TreeNodeFile("file", DateTime.Now, fhash);
            
            _fileSystem.AddFile("/file", new MockFileData("content"));
            
            var commitDto = new CommitDto()
            {
                Path = "/",
                Commit = commitModel,
                Tree = node,
                ItemsForWrite = new [] {"file"}
            };

            _treeStorageMock.Setup(s => s.Store(node)).Returns(Task.FromResult(thash));
            _blobStorageMock.Setup(s => s.Set(fhash, It.IsAny<Stream>())).Returns(Task.CompletedTask);
            _commitStorageMock.Setup(s => s.Store(commitModel)).Returns(Task.FromResult(chash));
            _flatTreeCreator.Setup(s => s.Create(It.IsAny<TreeNode>(), "/")).Returns(new [] {new FlatTreeItem("file", node)});
            
            // Act
            var result = await _commitStoreService.StoreCommit(commitDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(chash, result);
            _blobStorageMock.Verify(s => s.Set(fhash, It.IsAny<Stream>()), Times.Once);
        }
        
        [Test]
        public async Task Test_Store_ShouldStoreNoneBlob()
        {
            // Arrane
            var chash = "ca39a3ee5e6b4b0d3255bfef95601890afd80709";
            var thash = "ta39a3ee5e6b4b0d3255bfef95601890afd80709";
            
            var commitModel = new CommitModel()
            {
                Labels = new Dictionary<string, string>(),
            };
            var node = (TreeNode)new TreeNodeFile("", DateTime.Now);
            
            var commitDto = new CommitDto()
            {
                Path = "/",
                Commit = commitModel,
                Tree = node,
                ItemsForWrite = new string[0]
            };

            _treeStorageMock.Setup(s => s.Store(node)).Returns(Task.FromResult(thash));
            _commitStorageMock.Setup(s => s.Store(commitModel)).Returns(Task.FromResult(chash));
            
            // Act
            var result = await _commitStoreService.StoreCommit(commitDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(chash, result);
            _blobStorageMock.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<Stream>()), Times.Never);
        }

    }
}