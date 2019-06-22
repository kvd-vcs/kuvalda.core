using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Kuvalda.Core;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class HashTableCreatorTest
    {
        [Test]
        public async Task Test_Create_ShouldIgnoreFolder()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"/folder", new MockDirectoryData() },
            });
            var flatTree = new[]
            {
                new FlatTreeItem("folder", new TreeNodeFolder("folder")),
            };
            var hashProvider = new Mock<IHashComputeProvider>();
            var hashCreator = new HashTableCreator(fs, hashProvider.Object);
            
            // Act
            var result = await hashCreator.Compute(flatTree);
            
            // Assert
            Assert.AreEqual(0, result.Count);
        }
        
        [Test]
        public async Task Test_Create_ShouldComputeHashes()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"/file", new MockFileData("hash") },
            });
            var flatTree = new[]
            {
                new FlatTreeItem("file", new TreeNodeFile("file", DateTime.Today)),
            };
            var hashProvider = new Mock<IHashComputeProvider>();
            hashProvider.Setup(h => h.Compute(It.IsAny<Stream>()))
                .Returns(Task.FromResult("2346ad27d7568ba9896f1b7da6b5991251debdf2"));
            var hashCreator = new HashTableCreator(fs, hashProvider.Object);
            
            // Act
            var result = await hashCreator.Compute(flatTree);
            
            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("2346ad27d7568ba9896f1b7da6b5991251debdf2", result["file"]);
        }
    }
}