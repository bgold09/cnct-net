using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cnct.SourceGeneration;
using Microsoft.CodeAnalysis.Text;
using Xunit;

using VerifyCS = Cnct.SourceGeneration.Test.CSharpSourceGeneratorVerifier<Cnct.SourceGeneration.CnctTaskSpecificationGenerator>;

namespace Cnct.SourceGeneration.Test
{
    public class CnctTaskSpecificationGeneratorTests
    {
        [Fact]
        public async Task T()
        {
            var code = @"
using System;

namespace TestSpace
{
    public class HelloAttribute : Attribute
    {
        public string Something { get; set; }

        public HelloAttribute(string something)
        {
            this.Something = something;
        }
    }

    public interface ICnctActionSpec { void TestMethod(); }

    [Hello(""foo"")]
    public class Class1 : ICnctActionSpec {}
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code },
                    GeneratedSources =
                    {
                        (typeof(CnctTaskSpecificationGenerator), "GeneratedFileName", SourceText.From("", Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                    },
                },
            }.RunAsync();
        }

        public void T2()
        {
            int d = 0;
            Assert.Equal(0, d);
        }
    }
}
