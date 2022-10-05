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

        private const string CnctActionSpecInterfaceName = "ICnctActionSpec";

        public void Execute(GeneratorExecutionContext context)
        {
            const string CnctActionConverterClassName = "CnctActionConverter";
            const string AttributeName = "CnctActionType";
            var syntaxReceiver = context.SyntaxReceiver as ActionSpecSyntaxReceiver;
            var actionTypeToClassNameMap = new List<(string actionType, string className)>();
            foreach (ClassDeclarationSyntax cds in syntaxReceiver.ClassesToAugment)
            {
                AttributeSyntax result = cds.AttributeLists
                    .SelectMany(attributeList => attributeList.Attributes)
                    .SingleOrDefault(a => ((IdentifierNameSyntax)a.Name).Identifier.ValueText == AttributeName);

                if (result != null)
                {
                    AttributeArgumentSyntax attributeArgument = result.ArgumentList.Arguments.Single();
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
        private static {CnctActionSpecInterfaceName} GetActionSpecFromType(string actionType)
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
            context.RegisterForSyntaxNotifications(() => new ActionSpecSyntaxReceiver());
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

        private class ActionSpecSyntaxReceiver : ISyntaxReceiver
        {
            public ISet<ClassDeclarationSyntax> ClassesToAugment { get; } = new HashSet<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax cds &&
                        cds.BaseList?.Types
                            .Where(t => t.Type.IsKind(SyntaxKind.IdentifierName))
                            .Select(t => t.Type)
                            .Cast<IdentifierNameSyntax>()
                            .Any(t => t.Identifier.ValueText == CnctActionSpecInterfaceName) == true)
                {
                    this.ClassesToAugment.Add(cds);
                }
            }
        }
    }
}
