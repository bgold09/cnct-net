using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
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
            const string ActionRunnerClassName = "ActionRunner";
            const string AttributeName = "CnctActionType";
            var syntaxReceiver = context.SyntaxReceiver as ActionSpecSyntaxReceiver;
            var actionTypeToClassNameMap = new List<(string actionType, string className)>();
            var taskClassInfoList = new List<(string specClassName, string taskClassName, string taskNamespace, string accessibility)>();
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

                    string taskClassName = className.Replace("Specification", string.Empty);
                    var taskSymbol = context.Compilation
                        .GetSymbolsWithName(taskClassName, SymbolFilter.Type)
                        .OfType<INamedTypeSymbol>()
                        .FirstOrDefault();

                    if (taskSymbol != null)
                    {
                        string taskNamespace = taskSymbol.ContainingNamespace.ToDisplayString();
                        string accessibility = taskSymbol.DeclaredAccessibility == Accessibility.Public
                            ? "public"
                            : "internal";
                        taskClassInfoList.Add((className, taskClassName, taskNamespace, accessibility));
                        this.AddTaskGeneratedSource(context, className, taskClassName, taskNamespace, accessibility);
                    }
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

            this.AddActionRunnerGeneratedSource(context, ActionRunnerClassName, taskClassInfoList);
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
        public override string ActionType {{ get; }} = ""{actionType}"";
    }}
}}
";

            context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private void AddTaskGeneratedSource(
            GeneratorExecutionContext context,
            string specClassName,
            string taskClassName,
            string taskNamespace,
            string accessibility)
        {
            string source = @$"
namespace {taskNamespace}
{{
    {accessibility} partial class {taskClassName} : Cnct.Core.Tasks.CnctTaskBase
    {{
        public static partial {taskClassName} FromTaskSpecification(
            {NamespaceCnctCoreConfiguration}.{specClassName} spec,
            ILogger logger,
            string configDirectoryRoot);
    }}
}}
";

            context.AddSource($"{taskClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private void AddActionRunnerGeneratedSource(
            GeneratorExecutionContext context,
            string actionRunnerClassName,
            List<(string specClassName, string taskClassName, string taskNamespace, string accessibility)> taskClassInfoList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Cnct.Core.Configuration;");
            sb.AppendLine();
            sb.AppendLine("namespace Cnct.Core.Tasks");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {actionRunnerClassName}");
            sb.AppendLine("    {");
            sb.AppendLine("        private static async Task DispatchAsync(");
            sb.AppendLine("            ICnctActionSpec spec,");
            sb.AppendLine("            ILogger logger,");
            sb.AppendLine("            string configDirectoryRoot)");
            sb.AppendLine("        {");
            sb.AppendLine("            ICnctTask task = spec switch");
            sb.AppendLine("            {");

            foreach (var (specClassName, taskClassName, taskNamespace, _) in taskClassInfoList)
            {
                string fullTaskName = $"{taskNamespace}.{taskClassName}";
                sb.AppendLine($"                {specClassName} s => {fullTaskName}.FromTaskSpecification(s, logger, configDirectoryRoot),");
            }

            sb.AppendLine(@"                _ => throw new NotSupportedException($""Action type '{spec.ActionType}' is not supported.""),");
            sb.AppendLine("            };");
            sb.AppendLine("            await task.ExecuteAsync();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource(
                $"{actionRunnerClassName}.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private class ActionSpecSyntaxReceiver : ISyntaxReceiver
        {
            private const string CnctActionTypeAttributeName = "CnctActionType";

            public ISet<ClassDeclarationSyntax> ClassesToAugment { get; } = new HashSet<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax cds &&
                    cds.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Any(a => a.Name is IdentifierNameSyntax id &&
                                  id.Identifier.ValueText == CnctActionTypeAttributeName))
                {
                    this.ClassesToAugment.Add(cds);
                }
            }
        }
    }
}
