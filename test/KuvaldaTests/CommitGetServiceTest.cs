using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kuvalda.Core;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class CommitGetServiceTest
    {

        private ICommitGetService _commitGetService;
        
        private Mock<IEntityObjectStorage<CommitModel>> _commitStorageMock;
        private Mock<IEntityObjectStorage<IDictionary<string, string>>> _hashStorageMock;
        private Mock<IEntityObjectStorage<TreeNode>> _treeStorageMock;

        [SetUp]
        public void SetUp()
        {
            _commitStorageMock = new Mock<IEntityObjectStorage<CommitModel>>();
            _hashStorageMock = new Mock<IEntityObjectStorage<IDictionary<string, string>>>();
            _treeStorageMock = new Mock<IEntityObjectStorage<TreeNode>>();
            
            _commitGetService = new CommitGetService(_commitStorageMock.Object, _hashStorageMock.Object,
                _treeStorageMock.Object);
        }

        [Test]
        public async Task Test_Get_ShouldGetCommitData()
        {
            // Arrane
            var chash = "ca39a3ee5e6b4b0d3255bfef95601890afd80709";
            var commitModel = new CommitModel()
            {
                HashesAddress = "ha39a3ee5e6b4b0d3255bfef95601890afd80709",
                TreeHash = "ta39a3ee5e6b4b0d3255bfef95601890afd80709",
                Labels = new Dictionary<string, string>(),
            };
            var node = (TreeNode)new TreeNodeFile("", DateTime.Now);
            var hashes = Mock.Of<IDictionary<string, string>>();

            _commitStorageMock.Setup(storage => storage.Get(chash)).Returns(Task.FromResult(commitModel));
            _hashStorageMock.Setup(storage => storage.Get(commitModel.HashesAddress))
                .Returns(Task.FromResult(hashes));
            _treeStorageMock.Setup(storage => storage.Get(commitModel.TreeHash))
                .Returns(Task.FromResult(node));
            
            // Act
            var result = await _commitGetService.GetCommit(chash);
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(commitModel, result.Commit);
            Assert.AreEqual(node, result.Tree);
            Assert.AreEqual(hashes, result.Hashes);
        }
    }
}