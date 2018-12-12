using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Kuvalda.Tree;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class TreeFilterTest
    {
        [Test]
        public async Task Test_ShouldNotFilterIfNotIgnoreFile()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:/file", new MockFileData("content")},
            });
            var filter = new TreeFilter(fs);
            var tree = new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file", DateTime.UnixEpoch), 
                }
            };
            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(tree, filtered);
        }
        
        [Test]
        public async Task Test_ShouldIgnoreByMatchName()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:/file", new MockFileData("") {LastWriteTime = DateTimeOffset.UnixEpoch}},
                {@"c:/file1", new MockFileData("") {LastWriteTime = DateTimeOffset.UnixEpoch}},
                {$"c:/{TreeFilter.IgnoreFileName}", new MockFileData("[\"^file$\"]")},
            });
            var filter = new TreeFilter(fs);
            var tree = new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file", DateTime.UnixEpoch),
                    new TreeNodeFile("file1", DateTime.UnixEpoch),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.UnixEpoch),
                }
            };
            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file1", DateTime.UnixEpoch),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.UnixEpoch),
                }
            }, filtered);
        }
        
        [Test]
        public async Task Test_ShouldIgnorePredefinedNames()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:/file", new MockFileData("") {LastWriteTime = DateTimeOffset.UnixEpoch}},
                {@"c:/file1", new MockFileData("") {LastWriteTime = DateTimeOffset.UnixEpoch}},
            });
            
            var filter = new TreeFilter(fs);
            filter.PredefinedIgnores = new List<string>()
            {
                "file1"
            };
            
            var tree = new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file", DateTime.UnixEpoch),
                    new TreeNodeFile("file1", DateTime.UnixEpoch),
                }
            };
            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file", DateTime.UnixEpoch),
                }
            }, filtered);
        }
        
        [Test]
        public async Task Test_ShouldIgnorePredefinedNamesNotAffectIgnoreFiles()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:/file", new MockFileData("") {LastWriteTime = DateTimeOffset.UnixEpoch}},
                {@"c:/file1", new MockFileData("") {LastWriteTime = DateTimeOffset.UnixEpoch}},
                {$"c:/{TreeFilter.IgnoreFileName}", new MockFileData("[\"^file$\"]")},
            });
            
            var filter = new TreeFilter(fs);
            filter.PredefinedIgnores = new List<string>()
            {
                "file1"
            };

            var tree = new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file", DateTime.UnixEpoch),
                    new TreeNodeFile("file1", DateTime.UnixEpoch),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.UnixEpoch),
                }
            };

            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(new TreeNodeFolder("")
            {
                Nodes = new List<TreeNode>
                {
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.UnixEpoch)
                }
            }, filtered);
        }
    }
}