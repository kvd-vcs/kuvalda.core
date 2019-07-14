using System;
using Kuvalda.Core;
using Kuvalda.Core.Merge;
using Moq;
using NUnit.Framework;

namespace KuvaldaTests
{
    [TestFixture]
    public class ConflictDetectServiceUnitTests
    {
        private ConflictDetectService _service;
        private Mock<IDifferenceEntriesCreator> _diffCreator;

        [SetUp]
        public void SetUp()
        {
            _diffCreator = new Mock<IDifferenceEntriesCreator>();
            _service = new ConflictDetectService(_diffCreator.Object);
        }

        [Test]
        public void Test_Ctor_ThrowNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ConflictDetectService(null));
        }
    }
}