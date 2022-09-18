using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cnct.SourceGeneration
{
    [Generator]
    public class CnctTaskSpecificationGenerator : ISourceGenerator
    {
        private const string NamespaceCnctCoreConfiguration = "Cnct.Core.Configuration";

        public void Execute(GeneratorExecutionContext context)
        {
            const string CnctActionConverterClassName = "CnctActionConverter";
            const string AttributeName = "CnctActionType";
            var syntaxReceiver = context.SyntaxReceiver as MySyntaxReceiver;
            var actionTypeToClassNameMap = new List<(string actionType, string className)>();
            foreach (ClassDeclarationSyntax cds in syntaxReceiver.ClassesToAugment)
            {
                AttributeSyntax result = cds.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .SingleOrDefault(a => ((IdentifierNameSyntax)a.Name).Identifier.ValueText == AttributeName);

                if (result != null)
                {
                    AttributeArgumentSyntax arg = result.ArgumentList.Arguments.Single();
                    string value = result.ArgumentList.Arguments.Single().Expression.GetText().ToString();
                    value = value.Substring(1, value.Length - 2);

                    string className = cds.Identifier.ValueText;

                    this.AddActionSpecGeneratedSource(context, value, className);
                    actionTypeToClassNameMap.Add((value, className));
                }
            }

            StringBuilder actionConverterSourceBuilder = new(@$"using System;

namespace {NamespaceCnctCoreConfiguration}
{{
    public partial class {CnctActionConverterClassName}
    {{
        private static ICnctActionSpec GetActionSpecFromType(string actionType)
        {{
            return actionType switch
            {{
");

            foreach (var (actionType, className) in actionTypeToClassNameMap)
            {
                actionConverterSourceBuilder.AppendLine(
                    @$"               ""{actionType}"" => new {className}(),");
            }

            actionConverterSourceBuilder.AppendLine(@"                _ => throw new NotImplementedException(),
            };
        }
    }
}");

            context.AddSource(
                $"{CnctActionConverterClassName}.g.cs",
                SourceText.From(actionConverterSourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        private void AddActionSpecGeneratedSource(
            GeneratorExecutionContext context,
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

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public ISet<ClassDeclarationSyntax> ClassesToAugment { get; } = new HashSet<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                const string InterfaceName = "ICnctActionSpec";
                if (syntaxNode is ClassDeclarationSyntax cds &&
                         cds.BaseList?.Types
                            .Where(t => t.Type.Kind() == SyntaxKind.IdentifierName)
                            .Select(t => t.Type)
                            .Cast<IdentifierNameSyntax>()
                            .Any(t => t.Identifier.ValueText == InterfaceName) == true)
                {
                    this.ClassesToAugment.Add(cds);
                }
            }
        }
    }
}
