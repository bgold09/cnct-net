using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cnct.Core.Tasks;
using Moq;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CopyTaskTests
    {
        [Fact]
        public async Task CanCopyExistingFiles()
        {
            string sourceFile = "sourceFile";
            var destinations = new[] { "d1", "d2" };
            var map = new Dictionary<string, IEnumerable<string>>
            {
                [sourceFile] = destinations,
            };

            var fileMock = new Mock<IFile>(MockBehavior.Strict);
            fileMock.Setup(m => m.Exists(sourceFile)).Returns(true);
            foreach (var destination in destinations)
            {
                fileMock.Setup(m => m.Copy(sourceFile, destination));
            }

            var logger = Mock.Of<ILogger>();
            var fileSytemMock = Mock.Of<IFileSystem>(
                m => m.File == fileMock.Object,
                MockBehavior.Strict);

            var copyTask = new CopyTask(logger, fileSytemMock, map);

            await copyTask.ExecuteAsync();

            fileMock.Verify(m => m.Exists(sourceFile), Times.Once);
            fileMock.Verify(m => m.Exists(It.IsAny<string>()), Times.Once);
            fileMock.Verify(
                m => m.Copy(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(destinations.Length));

            foreach (var destination in destinations)
            {
                fileMock.Verify(m => m.Copy(sourceFile, destination), Times.Once);
            }
        }

        [Fact]
        public async Task ThrowsForNonexistentSourceFile()
        {
            string sourceFile = "sourceFile";
            var destinations = new[] { "d1", "d2" };
            var map = new Dictionary<string, IEnumerable<string>>
            {
                [sourceFile] = destinations,
            };

            var fileMock = new Mock<IFile>(MockBehavior.Strict);
            fileMock.Setup(m => m.Exists(sourceFile)).Returns(false);

            var logger = Mock.Of<ILogger>();
            var fileSytemMock = Mock.Of<IFileSystem>(
                m => m.File == fileMock.Object,
                MockBehavior.Strict);

            var copyTask = new CopyTask(logger, fileSytemMock, map);

            await copyTask.ExecuteAsync();

            fileMock.Verify(m => m.Exists(sourceFile), Times.Once);
            fileMock.Verify(m => m.Exists(It.IsAny<string>()), Times.Once);
            fileMock.Verify(
                m => m.Copy(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
