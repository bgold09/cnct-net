using System;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Cnct.Core.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string json = System.IO.File.ReadAllText(@"C:\Users\brian\.dotfiles\cnct.json");

            var config = JsonConvert.DeserializeObject<CnctConfig>(json);
            var logger = new ConsoleLogger(new LoggerOptions(true, true));

            config.ExecuteAsync(logger).ConfigureAwait(false);

            Console.WriteLine();
        }
    }
}
