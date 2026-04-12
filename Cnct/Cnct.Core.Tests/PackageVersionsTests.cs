using System.Xml.Linq;

namespace Cnct.Core.Tests
{
    public class PackageVersionsTests
    {
        [Fact]
        public void PowerShellPackages_ShouldHaveSameVersion()
        {
            var assembly = typeof(PackageVersionsTests).Assembly;
            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("Directory.Packages.props", StringComparison.OrdinalIgnoreCase));

            Assert.False(
                string.IsNullOrEmpty(resourceName),
                "Embedded resource 'Directory.Packages.props' not found in test assembly resources.");

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            Assert.NotNull(stream);

            var doc = XDocument.Load(stream);
            XNamespace ns = doc.Root.GetDefaultNamespace();

            var packageVersions = doc.Descendants("PackageVersion")
                .Select(e => new
                {
                    Include = (string)e.Attribute("Include"),
                    Version = (string)e.Attribute("Version"),
                })
                .Where(x => x.Include == "Microsoft.PowerShell.SDK" || x.Include == "System.Management.Automation")
                .ToList();

            Assert.Contains(packageVersions, p => p.Include == "Microsoft.PowerShell.SDK");
            Assert.Contains(packageVersions, p => p.Include == "System.Management.Automation");

            string powershellSdkPackageVersion =
                packageVersions.Single(p => p.Include == "Microsoft.PowerShell.SDK").Version;
            string systemManagementAutomationVersion =
                packageVersions.Single(p => p.Include == "System.Management.Automation").Version;

            Assert.Equal(powershellSdkPackageVersion, systemManagementAutomationVersion);
        }
    }
}
