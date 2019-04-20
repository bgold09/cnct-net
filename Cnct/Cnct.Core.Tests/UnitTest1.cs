using System;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string json = System.IO.File.ReadAllText(@"C:\Users\brian\.dotfiles\cnct.json");

            var config = JsonConvert.DeserializeObject<CnctConfig>(json);

            Console.WriteLine();
        }
    }
}
