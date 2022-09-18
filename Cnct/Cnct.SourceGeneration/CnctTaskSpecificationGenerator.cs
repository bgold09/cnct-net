using System;
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
        private const string AttributeName = "CnctActionType";

        private const string InterfaceName = "ICnctActionSpec";

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = context.SyntaxReceiver as MySyntaxReceiver;

            //var ns = context.Compilation.GlobalNamespace
            //    .GetNamespaceMembers()
            //    .Where(n => n.ContainingAssembly == context.Compilation.Assembly)
            //    .SelectMany(t => t.GetTypeMembers())
            //    .Where(t => t.TypeKind == TypeKind.Class)
            //    .ToArray();

            //context.Compilation.GlobalNamespace.GetNamespaceMembers().ToArray()[0].GetMembers().Count();

            //var n1 = ns.FirstOrDefault();
            //var interfaces = n1?.Interfaces;
            //interfaces.Value.FirstOrDefault().Name;

            //var classTypeSymbol = context.Compilation.GetSemanticModel().GetDeclaredSymbol(classDeclaration) as ITypeSymbol;

            //var classes = context.Compilation.Assembly.GlobalNamespace
            //    .GetTypeMembers()
            //    .Where(m => m.TypeKind == TypeKind.Class)
            //    .ToArray();

            //foreach (INamedTypeSymbol member in classes)
            //{
            //    var m2 = member.GetMembers("ActionType");
            //}

            var l = new List<(string actionType, string className)>();

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

                    this.G(context, value, className);
                    l.Add((value, className));
                }

                //var semantic = context.Compilation.GetSemanticModel(cds.SyntaxTree);
                //context.Compilation.GetEntryPoint().GetAttributes().First().AttributeClass.
                //var symbol = semantic.GetDeclaredSymbol(cds);
                //var attribute = symbol.GetAttributes().First();
                //symbol.GetAttributes().First().AttributeClass.Name;
                //var interfaces2 = symbol.Interfaces;
                //if (!interfaces2.Any(i => i.Name == "ICnctActionSpec"))
                //{
                //    continue;
                //}
            }

            StringBuilder builder = new(@$"using System;

namespace Cnct.Core.Configuration
{{
    public partial class CnctActionConverter
    {{
        private static ICnctActionSpec GetActionSpecFromType(string actionType)
        {{
            return actionType switch
            {{
");

            foreach (var (actionType, className) in l)
            {
                builder.AppendLine(@$"               ""{actionType}"" => new {className}(),");
            }

            builder.AppendLine(@"                _ => throw new NotImplementedException(),
            };
        }
    }
}");

            context.AddSource("CnctActionConverter.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));

            //ICnctActionSpec spec = jsonObject["actionType"].Value<string>() switch
            //{
            //    "link" => new LinkTaskSpecification(),
            //    "shell" => new ShellTaskSpecification(),
            //    "environmentVariable" => new EnvironmentVariableTaskSpecification(),
            //    _ => throw new NotImplementedException(),
            //};

        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        private void G(GeneratorExecutionContext context, string val, string className)
        {
            StringBuilder builder = new(@$"
namespace Cnct.Core.Configuration
{{
    public partial class {className}
    {{
        public string ActionType {{ get; }} = ""{val}"";
    }}
}}
");

            context.AddSource($"{className}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public ISet<ClassDeclarationSyntax> ClassesToAugment { get; } = new HashSet<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax cds)
                {
                    if (cds.BaseList?.Types
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
}
