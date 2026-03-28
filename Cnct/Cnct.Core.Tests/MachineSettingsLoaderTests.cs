using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class MachineSettingsLoaderTests
    {
        [Fact]
        public async Task LoadAsync_ReturnsMachineSettingsEmpty_WhenFileDoesNotExist()
        {
            var mockFileSystem = new MockFileSystem();
            var loader = new MachineSettingsLoader(mockFileSystem);

            MachineSettings result = await loader.LoadAsync();

            Assert.Same(MachineSettings.Empty, result);
        }

        [Fact]
        public async Task LoadAsync_ReturnsTags_WhenFileExistsWithTags()
        {
            string path = MachineSettingsLoader.GetSettingsFilePath();
            string json = JsonConvert.SerializeObject(new { tags = new[] { "personal", "home" } });
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(mockFileSystem.Path.GetDirectoryName(path));
            mockFileSystem.File.WriteAllText(path, json);

            var loader = new MachineSettingsLoader(mockFileSystem);
            MachineSettings result = await loader.LoadAsync();

            Assert.Contains("personal", result.Tags);
            Assert.Contains("home", result.Tags);
        }

        [Fact]
        public async Task LoadAsync_ReturnsSingleTag_WhenFileHasSingleStringTag()
        {
            string path = MachineSettingsLoader.GetSettingsFilePath();
            string json = JsonConvert.SerializeObject(new { tags = "work" });
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(mockFileSystem.Path.GetDirectoryName(path));
            mockFileSystem.File.WriteAllText(path, json);

            var loader = new MachineSettingsLoader(mockFileSystem);
            MachineSettings result = await loader.LoadAsync();

            Assert.Single(result.Tags, "work");
        }

        [Fact]
        public async Task LoadAsync_ReturnsEmptyTags_WhenFileHasNoTags()
        {
            string path = MachineSettingsLoader.GetSettingsFilePath();
            string json = "{}";
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(mockFileSystem.Path.GetDirectoryName(path));
            mockFileSystem.File.WriteAllText(path, json);

            var loader = new MachineSettingsLoader(mockFileSystem);
            MachineSettings result = await loader.LoadAsync();

            Assert.Empty(result.Tags);
        }
    }
}
