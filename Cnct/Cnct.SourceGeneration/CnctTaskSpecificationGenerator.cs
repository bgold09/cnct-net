using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cnct.SourceGeneration
{
    [Generator]
    public class CnctTaskSpecificationGenerator : IIncrementalGenerator
    {
        private const string NamespaceCnctCoreConfiguration = "Cnct.Core.Configuration";

        private const string CnctActionSpecInterfaceName = "ICnctActionSpec";

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            const string CnctActionConverterClassName = "CnctActionConverter";
            const string AttributeName = "CnctActionType";

            Func<SyntaxNode, (string, string)?> g2 = (c) =>
            {
                var cds = (ClassDeclarationSyntax)c;

                AttributeSyntax result = cds.AttributeLists
                    .SelectMany(attributeList => attributeList.Attributes)
                    .SingleOrDefault(a => ((IdentifierNameSyntax)a.Name).Identifier.ValueText == AttributeName);

                if (result == null)
                {
                    return null;
                }

                AttributeArgumentSyntax attributeArgument = result.ArgumentList.Arguments.Single();
                string actionType = result.ArgumentList.Arguments.Single().Expression.GetText().ToString();
                actionType = actionType.Substring(1, actionType.Length - 2);

                string className = cds.Identifier.ValueText;

                return (actionType, className);
            };

            var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
                static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax cds &&
                        cds.BaseList?.Types
                            .Where(t => t.Type.IsKind(SyntaxKind.IdentifierName))
                            .Select(t => t.Type)
                            .Cast<IdentifierNameSyntax>()
                            .Any(t => t.Identifier.ValueText == CnctActionSpecInterfaceName) == true,
                static (c, _) => c.Node);

            var p2 = syntaxProvider
                .Select((n, ct) => g2(n))
                .Where(n => n != null);

            var p3 = p2.Collect();

            context.RegisterSourceOutput(p3, (spc, actionTypeAndClassName) =>
            {
            StringBuilder actionConverterSourceBuilder = new(@$"using System;

namespace {NamespaceCnctCoreConfiguration}
{{
    public partial class {CnctActionConverterClassName}
    {{
        private static {CnctActionSpecInterfaceName} GetActionSpecFromType(string actionType)
        {{
            return actionType switch
            {{
");

            var nonNull = actionTypeAndClassName.Where(s => s != null).Select(s => s.Value);

            foreach (var (actionType, className) in nonNull)
            {
                AddActionSpecGeneratedSource(spc, actionType, className);
                actionConverterSourceBuilder.AppendLine(
                    @$"               ""{actionType}"" => new {className}(),");
            }

            actionConverterSourceBuilder.AppendLine(@"                _ => throw new NotImplementedException(),
            };
        }
    }
}");

                spc.AddSource(
                    $"{CnctActionConverterClassName}.g.cs",
                    SourceText.From(actionConverterSourceBuilder.ToString(), Encoding.UTF8));
            });
        }

        private static void AddActionSpecGeneratedSource(
            SourceProductionContext context,
            string actionType,
            string className)
        {
            string source = @$"
namespace {NamespaceCnctCoreConfiguration}
{{
    public partial class {className}
    {{
        public string ActionType {{ get; }} = ""{actionType}"";
    }}
}}
";

            context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }
}
