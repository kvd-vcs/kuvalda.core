using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Kuvalda.Core;
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
                    new TreeNodeFile("file", DateTime.Today), 
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
                {@"c:/file", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {@"c:/file1", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {$"c:/{TreeFilter.IgnoreFileName}", new MockFileData("^file$")},
            });
            var filter = new TreeFilter(fs);
            var tree = new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file", DateTime.Today),
                    new TreeNodeFile("file1", DateTime.Today),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today),
                }
            };
            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file1", DateTime.Today),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today),
                }
            }, filtered);
        }
        
        [Test]
        public async Task Test_ShouldIgnorePredefinedNames()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:/file", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {@"c:/file1", new MockFileData("") {LastWriteTime = DateTime.Today}},
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
                    new TreeNodeFile("file", DateTime.Today),
                    new TreeNodeFile("file1", DateTime.Today),
                }
            };
            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(new TreeNodeFolder("")
            {
                Nodes = new []
                {
                    new TreeNodeFile("file", DateTime.Today),
                }
            }, filtered);
        }
        
        [Test]
        public async Task Test_ShouldIgnorePredefinedNamesNotAffectIgnoreFiles()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:/file", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {@"c:/file1", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {$"c:/{TreeFilter.IgnoreFileName}", new MockFileData("^file$")},
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
                    new TreeNodeFile("file", DateTime.Today),
                    new TreeNodeFile("file1", DateTime.Today),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today),
                }
            };

            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(new TreeNodeFolder("")
            {
                Nodes = new List<TreeNode>
                {
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today)
                }
            }, filtered);
        }
        
        [Test]
        public async Task Test_ShouldIgnoreHierarchy()
        {
            // Arrange
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {$"c:/{TreeFilter.IgnoreFileName}", new MockFileData("^file$")},
                {@"c:/file", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {@"c:/file1", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {@"c:/folder", new MockDirectoryData()},
                {@"c:/folder/file", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {@"c:/folder/file1", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {$"c:/folder/{TreeFilter.IgnoreFileName}", new MockFileData("^file$")},
                {@"c:/deep_folder", new MockDirectoryData()},
                {@"c:/deep_folder/deep_folder", new MockDirectoryData()},
                {@"c:/deep_folder/deep_folder/file", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {$"c:/deep_folder/deep_folder/{TreeFilter.IgnoreFileName}", new MockFileData("^deep_folder$")},
                {@"c:/deep_folder/deep_folder/deep_folder", new MockDirectoryData()},
                {@"c:/deep_folder/deep_folder/deep_folder/file", new MockFileData("") {LastWriteTime = DateTime.Today}},
                {@"c:/deep_folder/deep_folder/deep_folder/file1", new MockFileData("") {LastWriteTime = DateTime.Today}},
            });
            var filter = new TreeFilter(fs);
            var tree = new TreeNodeFolder("")
            {
                Nodes = new TreeNode[]
                {
                    new TreeNodeFile("file1", DateTime.Today),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today),
                    new TreeNodeFolder("folder")
                    {
                        Nodes = new []
                        {
                            new TreeNodeFile("file1", DateTime.Today),
                            new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today)
                        }
                    },
                    new TreeNodeFolder("deep_folder")
                    {
                        Nodes = new TreeNode[]
                        {
                            new TreeNodeFile("file", DateTime.Today),
                            new TreeNodeFolder("deep_folder")
                            {
                                Nodes = new TreeNode[]
                                {
                                    new TreeNodeFile("file", DateTime.Today),
                                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today),
                                    new TreeNodeFolder("deep_folder")
                                    {
                                        Nodes = new []
                                        {
                                            new TreeNodeFile("file1", DateTime.Today),
                                            new TreeNodeFile("file", DateTime.Today)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
            // Act
            var filtered = await filter.Filter(tree, "c:/");
            
            // Assert
            Assert.AreEqual(new TreeNodeFolder("")
            {
                Nodes = new TreeNode[]
                {
                    new TreeNodeFile("file1", DateTime.Today),
                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today),
                    new TreeNodeFolder("folder")
                    {
                        Nodes = new []
                        {
                            new TreeNodeFile("file1", DateTime.Today),
                            new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today)
                        }
                    },
                    new TreeNodeFolder("deep_folder")
                    {
                        Nodes = new TreeNode[]
                        {
                            new TreeNodeFile("file", DateTime.Today),
                            new TreeNodeFolder("deep_folder")
                            {
                                Nodes = new TreeNode[]
                                {
                                    new TreeNodeFile("file", DateTime.Today),
                                    new TreeNodeFile(TreeFilter.IgnoreFileName, DateTime.Today)
                                }
                            }
                        }
                    }
                }
            }, filtered);
        }

    }
}