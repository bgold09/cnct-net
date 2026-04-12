using System.IO.Abstractions;
using Cnct.Core.Tasks;

namespace Cnct.Core.Tests
{
    public class CopyTaskTests
    {
        [Fact]
        public async Task CanCopyExistingFiles()
        {
            string sourceFile = "sourceFile";
            string[] destinations = ["d1", "d2"];
            var map = new Dictionary<string, IEnumerable<string>>
            {
                [sourceFile] = destinations,
            };

            var fileMock = new Mock<IFile>(MockBehavior.Strict);
            fileMock.Setup(m => m.Exists(sourceFile)).Returns(true);
            foreach (var destination in destinations)
            {
                fileMock.Setup(m => m.Exists(destination)).Returns(false);
                fileMock.Setup(m => m.Copy(sourceFile, destination));
            }

            var logger = Mock.Of<ILogger>();
            var fileSytemMock = Mock.Of<IFileSystem>(
                m => m.File == fileMock.Object,
                MockBehavior.Strict);

            var copyTask = new CopyTask(logger, fileSytemMock, map);

            await copyTask.ExecuteAsync();

            fileMock.Verify(m => m.Exists(sourceFile), Times.Once);
            fileMock.Verify(m => m.Exists(It.IsAny<string>()), Times.Exactly(destinations.Length + 1));
            fileMock.Verify(
                m => m.Copy(It.IsAny<string>(), It.IsAny<string>()),
                Times.Exactly(destinations.Length));

            foreach (var destination in destinations)
            {
                fileMock.Verify(m => m.Exists(destination), Times.Once);
                fileMock.Verify(m => m.Copy(sourceFile, destination), Times.Once);
            }
        }

        [Fact]
        public async Task ThrowsForNonexistentSourceFile()
        {
            string sourceFile = "sourceFile";
            string[] destinations = ["d1", "d2"];
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

        [Fact]
        public async Task DoesNotOverwriteExistingDestinationFile()
        {
            string sourceFile = "sourceFile";
            string existingDestinationFile = "ed1";
            string nonexistentDestinationFile = "ned1";
            string[] destinations = [existingDestinationFile, nonexistentDestinationFile];
            var map = new Dictionary<string, IEnumerable<string>>
            {
                [sourceFile] = destinations,
            };

            var fileMock = new Mock<IFile>(MockBehavior.Strict);
            fileMock.Setup(m => m.Exists(sourceFile)).Returns(true);
            fileMock.Setup(m => m.Exists(existingDestinationFile)).Returns(true);
            fileMock.Setup(m => m.Exists(nonexistentDestinationFile)).Returns(false);
            fileMock.Setup(m => m.Copy(sourceFile, nonexistentDestinationFile));

            var logger = Mock.Of<ILogger>();
            var fileSytemMock = Mock.Of<IFileSystem>(
                m => m.File == fileMock.Object,
                MockBehavior.Strict);

            var copyTask = new CopyTask(logger, fileSytemMock, map);

            await copyTask.ExecuteAsync();

            fileMock.Verify(m => m.Exists(sourceFile), Times.Once);
            fileMock.Verify(m => m.Exists(existingDestinationFile), Times.Once);
            fileMock.Verify(m => m.Exists(nonexistentDestinationFile), Times.Once);
            fileMock.Verify(m => m.Exists(It.IsAny<string>()), Times.Exactly(destinations.Length + 1));
            fileMock.Verify(m => m.Copy(sourceFile, nonexistentDestinationFile), Times.Once);
            fileMock.Verify(m => m.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
