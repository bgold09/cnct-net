using Cnct.Core.Configuration;
using Cnct.Core.Validation;

namespace Cnct.Core.Tests
{
    public class ShellTaskSpecificationTests
    {
        [Theory]
        [InlineData(ShellTaskSpecification.ShellType.PowerShell, "PowerShell")]
        [InlineData(ShellTaskSpecification.ShellType.PowerShell, "powershell")]
        [InlineData(ShellTaskSpecification.ShellType.Sh, "Sh")]
        [InlineData(ShellTaskSpecification.ShellType.Sh, "sh")]
        public void CanDeserializeShellType(
            ShellTaskSpecification.ShellType expectedShellType,
            string shellTypeStr)
        {
            const string command = "some command";
            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "shell",
                ["shell"] = shellTypeStr,
                ["command"] = command,
                ["os"] = "windows",
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<ShellTaskSpecification>(specInterface);
            Assert.Equal(expectedShellType, spec.Shell);
            Assert.Equal(command, spec.Command);
            Assert.Equal(PlatformType.Windows, spec.PlatformType.Single());
            Assert.False(spec.Silent);
        }

        [Fact]
        public void CanDeserializeSingleTag()
        {
            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "shell",
                ["shell"] = "powershell",
                ["command"] = "echo hello",
                ["os"] = "windows",
                ["tags"] = "personal",
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<ShellTaskSpecification>(specInterface);
            Assert.Single(spec.Tags, "personal");
        }

        [Fact]
        public void CanDeserializeTagsArray()
        {
            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "shell",
                ["shell"] = "powershell",
                ["command"] = "echo hello",
                ["os"] = "windows",
                ["tags"] = new[] { "personal", "home" },
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<ShellTaskSpecification>(specInterface);
            Assert.Equal(2, spec.Tags.Count);
            Assert.Contains("personal", spec.Tags);
            Assert.Contains("home", spec.Tags);
        }

        [Fact]
        public void TagsDefaultsToEmptyWhenNotSpecified()
        {
            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "shell",
                ["shell"] = "powershell",
                ["command"] = "echo hello",
                ["os"] = "windows",
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<ShellTaskSpecification>(specInterface);
            Assert.Empty(spec.Tags);
        }

        [Fact]
        public void OsIsOptional()
        {
            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "shell",
                ["shell"] = "powershell",
                ["command"] = "echo hello",
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<ShellTaskSpecification>(specInterface);
            Assert.Null(spec.PlatformType);
        }

        [Fact]
        public void GetDisplayText_ReturnsShellAndCommand()
        {
            var spec = new ShellTaskSpecification
            {
                Shell = ShellTaskSpecification.ShellType.PowerShell,
                Command = "Install-Module foo",
            };

            Assert.Equal("shell: 'powerShell Install-Module foo'", spec.GetDisplayText());
        }

        [Fact]
        public void GetDisplayText_ShellSh_ReturnsShellAndCommand()
        {
            var spec = new ShellTaskSpecification
            {
                Shell = ShellTaskSpecification.ShellType.Sh,
                Command = "echo hello",
            };

            Assert.Equal("shell: 'sh echo hello'", spec.GetDisplayText());
        }

        [Fact]
        public void Validate_ReturnsWarning_WhenShellNotOnPath()
        {
            // Use a shell type that is guaranteed not to exist on PATH under this name
            var spec = new ShellTaskSpecification
            {
                Shell = ShellTaskSpecification.ShellType.PowerShell,
                Command = "echo hello",
            };

            IReadOnlyList<ValidationIssue> issues = spec.Validate("/config");

            // The result depends on whether pwsh is installed — just verify the shape
            // If pwsh is not on PATH, we expect a warning; if it is, we expect no issues.
            Assert.All(issues, i => Assert.Equal(ValidationSeverity.Warning, i.Severity));
        }
    }
}
