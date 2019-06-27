using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kuvalda.Core;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class CommitCreateServiceTests
    {
        private CommitCreateService _commitCreateService;
        
        private Mock<IEntityObjectStorage<CommitModel>> _commitStorageMock;
        private Mock<IEntityObjectStorage<TreeNode>> _treeStorageMock;
        private Mock<ITreeCreator> _treeCreatorMock;
        private Mock<IHashModificationFactory> _hashesFactoryMock;
        private Mock<IFlatTreeCreator> _flatTreeCreator;

        [SetUp]
        public void SetUp()
        {
            _commitStorageMock = new Mock<IEntityObjectStorage<CommitModel>>();
            _treeStorageMock = new Mock<IEntityObjectStorage<TreeNode>>();
            _treeCreatorMock = new Mock<ITreeCreator>();
            _hashesFactoryMock = new Mock<IHashModificationFactory>();
            _flatTreeCreator = new Mock<IFlatTreeCreator>();

            _commitCreateService = new CommitCreateService(_commitStorageMock.Object, _treeCreatorMock.Object,
                _treeStorageMock.Object, _hashesFactoryMock.Object, _flatTreeCreator.Object);
        }

        [Test]
        public async Task Test_Create_ShouldCreateCommitDataWithParent()
        {
            // Arrange
            var chashPrev = "ca39a3ee5e6b4b0d3255bfef95601890afd80709";
            var prevCommit = new CommitModel()
            {
                TreeHash = "ta39a3ee5e6b4b0d3255bfef95601890afd80709"
            };
            TreeNode currentNode = new TreeNodeFile("file", DateTime.Now);
            TreeNode prevNode = new TreeNodeFile("file", DateTime.Now);
            IDictionary<string, string> hashes = new Dictionary<string, string>();
            
            _treeCreatorMock.Setup(s => s.Create("/")).Returns(Task.FromResult(currentNode));
            _commitStorageMock.Setup(s => s.Get(chashPrev)).Returns(Task.FromResult(prevCommit));
            _treeStorageMock.Setup(s => s.Get(prevCommit.TreeHash)).Returns(Task.FromResult(prevNode));
            _hashesFactoryMock.Setup(s => s.CreateHashes(prevNode, currentNode, "/")).Returns(Task.FromResult(hashes));
            _flatTreeCreator.Setup(s => s.Create(currentNode, "/"))
                .Returns(new [] {new FlatTreeItem(currentNode.Name, currentNode)});

            _flatTreeCreator.Setup(s => s.Create(prevNode, "/"))
                .Returns(new [] {new FlatTreeItem(prevNode.Name, prevNode)});
            
            // Act
            var result = await _commitCreateService.CreateCommit("/", chashPrev);
            
            // Assert
            Assert.NotNull(result);
            
            Assert.AreEqual("/", result.Path);
            Assert.AreEqual(currentNode, result.Tree);
            Assert.AreEqual(hashes.Keys, result.ItemsForWrite);
        }
        
        [Test]
        public async Task Test_Create_ShouldCreateCommitDataWithoutParent()
        {
            // Arrange
            TreeNode currentNode = new TreeNodeFile("file", DateTime.Now);
            IDictionary<string, string> hashes = new Dictionary<string, string>();
            
            _treeCreatorMock.Setup(s => s.Create("/")).Returns(Task.FromResult(currentNode));
            _hashesFactoryMock.Setup(s => s.CreateHashes(It.IsAny<TreeNodeFolder>(), currentNode, "/"))
                .Returns(Task.FromResult(hashes));
            
            _flatTreeCreator.Setup(s => s.Create(currentNode, "/"))
                .Returns(new [] {new FlatTreeItem(currentNode.Name, currentNode)});

            _flatTreeCreator.Setup(s => s.Create(new TreeNodeFolder(""), "/"))
                .Returns(new [] {new FlatTreeItem("", new TreeNodeFolder(""))});
            
            // Act
            var result = await _commitCreateService.CreateCommit("/");
            
            // Assert
            Assert.NotNull(result);
            
            Assert.AreEqual("/", result.Path);
            Assert.AreEqual(currentNode, result.Tree);
            Assert.AreEqual(hashes.Keys, result.ItemsForWrite);
        }
    }
}