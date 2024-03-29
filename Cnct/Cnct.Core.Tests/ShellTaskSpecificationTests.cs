﻿using System.Collections.Generic;
using System.Linq;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class ShellTaskSpecificationTests
    {
        [Theory]
        [InlineData(ShellTaskSpecification.ShellType.PowerShell, "PowerShell")]
        [InlineData(ShellTaskSpecification.ShellType.PowerShell, "powershell")]
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
    }
}
