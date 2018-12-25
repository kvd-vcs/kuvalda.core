using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using Kuvalda.Core;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class HashTableCreatorTest
    {
        [Test]
        public void Test_Create_ShouldIgnoreFolder()
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
            var hashCreator = new HashTableCreator(fs, () => new SHA1Managed());
            
            // Act
            var result = hashCreator.Compute(flatTree);
            
            // Assert
            Assert.AreEqual(0, result.Count);
        }
        
        [Test]
        public void Test_Create_ShouldComputeHashes()
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
            var hashCreator = new HashTableCreator(fs, () => new SHA1Managed());
            
            // Act
            var result = hashCreator.Compute(flatTree);
            
            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("2346ad27d7568ba9896f1b7da6b5991251debdf2", result["file"]);
        }
    }
}