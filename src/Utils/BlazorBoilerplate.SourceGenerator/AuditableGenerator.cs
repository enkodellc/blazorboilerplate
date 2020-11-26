using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorBoilerplate.SourceGenerator
{
    [Generator]
    public class AuditableGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;

            INamedTypeSymbol interfaceSymbol = context.Compilation.GetTypeByMetadataName("BlazorBoilerplate.Infrastructure.Storage.DataInterfaces.IAuditable");

            foreach (var c in receiver.CandidateClasses)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(c.SyntaxTree);

                INamedTypeSymbol nameSymbol = model.GetDeclaredSymbol(c) as INamedTypeSymbol;
                if (nameSymbol.Interfaces.Any(i => i.Equals(interfaceSymbol, SymbolEqualityComparer.Default)))
                {
                    string namespaceName = nameSymbol.ContainingNamespace.ToDisplayString();

                    string source = $@"
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace {namespaceName}
{{
    public partial class {nameSymbol.Name}
    {{
        [Column(TypeName = ""datetime2(7)"")]
        public DateTime CreatedOn {{ get; set; }}

        [Column(TypeName = ""datetime2(7)"")]
        public DateTime? ModifiedOn {{ get; set; }}

        public ApplicationUser CreatedBy {{ get; set; }}

        public Guid? CreatedById {{ get; set; }}

        public ApplicationUser ModifiedBy {{ get; set; }}

        public Guid? ModifiedById {{ get; set; }}
    }}
}}
";

                    context.AddSource($"{nameSymbol.Name}_auditable.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}
