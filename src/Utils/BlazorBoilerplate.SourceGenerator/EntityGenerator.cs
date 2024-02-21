using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlazorBoilerplate.SourceGenerator
{
    [Generator]
    public class EntityGenerator : ISourceGenerator
    {
        private List<string> typeNames;
        public void Execute(GeneratorExecutionContext context)
        {
            var config = GetConfig(context);
            if (config == null)
                return;

            var syntaxTrees = ManualLoad(config.EntitiesPath);

            typeNames = syntaxTrees
                .Select(i => (ClassDeclarationSyntax)i.GetRoot().DescendantNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.ClassDeclaration)))
                .Where(i => i != null)
                .Select(i => i.Identifier.ValueText)
                .ToList();

            foreach (SyntaxTree tree in syntaxTrees)
            {
                var typeText = GetTypeText(context, tree, config, out var fileName);
                if (typeText == null)
                    continue;

                context.AddSource(fileName, SourceText.From(typeText, Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif 
        }

        private string GetTypeText(GeneratorExecutionContext context, SyntaxTree file, EntityGeneratorConfig config, out string fileName)
        {
            string classText = null;
            fileName = null;
            var root = file.GetRoot();

            var classDeclaration = (ClassDeclarationSyntax)root.DescendantNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.ClassDeclaration));
            if (classDeclaration == null)
                return null;

            var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                .Where(x => !x.AttributeLists.SelectMany(x => x.Attributes).Any(a => a.Name.ToString() == "JsonIgnore"));

            var propertiesSb = new StringBuilder();

            var typeName = classDeclaration.Identifier.ValueText;

            var compilation = CSharpCompilation.Create(typeName)
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(file);

            if (classDeclaration.BaseList?.Types.Any(i => i.Type is IdentifierNameSyntax && ((IdentifierNameSyntax)i.Type).Identifier.ValueText == "IAuditable") == true)
            {
                if (config.GenEntities)
                    propertiesSb.Append(@"
        public DateTime CreatedOn
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }

        public DateTime? ModifiedOn
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }

        public ApplicationUser CreatedBy
        {
            get { return GetValue<ApplicationUser>(); }
            set { SetValue(value); }
        }

        public Guid? CreatedById
        {
            get { return GetValue<Guid?>(); }
            set { SetValue(value); }
        }

        public ApplicationUser ModifiedBy
        {
            get { return GetValue<ApplicationUser>(); }
            set { SetValue(value); }
        }

        public Guid? ModifiedById
        {
            get { return GetValue<Guid?>(); }
            set { SetValue(value); }
        }
");
                if (config.GenEntities && config.EntityWithInterface)
                    propertiesSb.Append(@$"
        IApplicationUser I{typeName}.CreatedBy {{ get => CreatedBy; set => CreatedBy = (ApplicationUser)value; }}

        IApplicationUser I{typeName}.ModifiedBy {{ get => ModifiedBy; set => ModifiedBy = (ApplicationUser)value; }}
");
                if (config.GenInterfaces)
                    propertiesSb.Append(@"
        DateTime CreatedOn { get; set; }

        DateTime? ModifiedOn { get; set; }

        IApplicationUser CreatedBy { get; set; }

        Guid? CreatedById { get; set; }

        IApplicationUser ModifiedBy { get; set; }

        Guid? ModifiedById { get; set; }
");
            }

            foreach (var pds in properties)
            {
                var property = compilation
                    .GetSemanticModel(pds.SyntaxTree)
                    .GetDeclaredSymbol(pds);

                if (property.DeclaredAccessibility != Accessibility.Public)
                    continue;

                var propertyType = property.Type as INamedTypeSymbol;

                var setter = @"
            set { SetValue(value); }";

                string propertyTypeName = propertyType != null ? propertyType.Name : ((IArrayTypeSymbol)((IArrayTypeSymbol)property.Type).OriginalDefinition).ElementType.Name + "[]";

                if (propertyType?.IsGenericType == true)
                {
                    propertyTypeName = GetFirstGenericTypeName(pds, property);

                    if (propertyType.NullableAnnotation == NullableAnnotation.Annotated)
                        propertyTypeName = $"{propertyTypeName}?";
                    else if (propertyType.Name == "ICollection")
                    {
                        if (config.GenInterfaces)
                            propertiesSb.Append(@$"
        ICollection<I{propertyTypeName}> {property.Name} {{ get; }}
");
                        if (config.GenEntities && config.EntityWithInterface)
                            propertiesSb.Append(@$"
        ICollection<I{propertyTypeName}> I{typeName}.{property.Name} {{ get => {property.Name}.Select(i => (I{propertyTypeName})i).ToList(); }}
");
                        propertyTypeName = $"NavigationSet<{propertyTypeName}>";
                        setter = string.Empty;
                    }
                }
                else
                {
                    if (typeNames.Contains(propertyTypeName))
                    {
                        var iPropertyTypeName = $"I{propertyTypeName}";

                        if (config.GenEntities && config.EntityWithInterface)
                            propertiesSb.Append(@$"
        {iPropertyTypeName} I{typeName}.{property.Name} {{ get => {property.Name}; set => {property.Name} = ({propertyTypeName})value; }}
");


                        if (config.GenInterfaces)
                            propertiesSb.Append(@$"
        {iPropertyTypeName} {property.Name} {{ get; set; }}
");
                    }
                    else if (config.GenInterfaces)
                        propertiesSb.Append(@$"
        {propertyTypeName} {property.Name} {{ get; set; }}
");
                }

                if (config.GenEntities && config.EntityWithInterface)
                    propertiesSb.Append(@$"
        public {propertyTypeName} {property.Name}
        {{
            get {{ return GetValue<{propertyTypeName}>(); }}{setter}
        }}
");
                if (config.GenEntities && !config.EntityWithInterface)
                    propertiesSb.Append(@$"
        public {propertyTypeName} {property.Name}
        {{
            get {{ return GetValue<{propertyTypeName}>(); }}{setter}
        }}
");
            }

            fileName = $"{typeName}.g.cs";

            if (config.GenInterfaces)
            {
                fileName = $"I{typeName}.g.cs";

                classText = @$"//Autogenerated by EntityGenerator
using BlazorBoilerplate.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlazorBoilerplate.Shared.DataInterfaces
{{
    public interface I{typeName}
    {{{propertiesSb}
    }}
}}
";
            }

            if (config.GenEntities && config.EntityWithInterface)
                classText = @$"//Autogenerated by EntityGenerator
using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Shared.DataInterfaces;
using Breeze.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BlazorBoilerplate.Shared.Dto.Db
{{
    public partial class {typeName} : BaseEntity, I{typeName}
    {{{propertiesSb}
    }}
}}
";

            if (config.GenEntities && !config.EntityWithInterface)
                classText = @$"//Autogenerated by EntityGenerator
using BlazorBoilerplate.Constants;
using Breeze.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BlazorBoilerplate.Shared.Dto.Db
{{
    public partial class {typeName} : BaseEntity
    {{{propertiesSb}
    }}
}}
";

            return classText;
        }


        private IEnumerable<SyntaxTree> ManualLoad(string rootDir)
        {
            foreach (var filepath in Directory.GetFiles(rootDir, "*.cs", SearchOption.AllDirectories))
            {
                var file = File.ReadAllText(filepath);
                yield return CSharpSyntaxTree.ParseText(file);
            }
        }

        private EntityGeneratorConfig GetConfig(GeneratorExecutionContext context)
        {
            var configFile = context.AdditionalFiles.FirstOrDefault(x => Path.GetFileName(x.Path) == "EntityGeneratorConfig.json");
            if (configFile == null)
                return null;

            var jsonString = File.ReadAllText(configFile.Path);
            var config = JsonConvert.DeserializeObject<EntityGeneratorConfig>(jsonString);

            config.EntitiesPath = CrossPlatform.PathCombine(Path.GetDirectoryName(configFile.Path), config.EntitiesPath.Split("\\".ToCharArray()));

            return config;
        }

        private static string BuildTypeName(TypeSyntax node, IPropertySymbol property)
        {
            if (node is NullableTypeSyntax nts)
                node = nts.ElementType;

            if (node is IdentifierNameSyntax ins)
            {
                var namedType = property.Type as INamedTypeSymbol;
                var typeArg = namedType!.TypeArguments
                    .First(x => x.Name == ins.Identifier.ValueText);

                return ins.Identifier.ValueText;
            }

            if (node is PredefinedTypeSyntax pts)
                return pts.Keyword.ValueText;

            return default;
        }

        static string GetFirstGenericTypeName(
            PropertyDeclarationSyntax pds,
            IPropertySymbol property)
        {
            var gns = pds.DescendantNodes()
                .OfType<GenericNameSyntax>();

            var type = pds.Type;

            if (gns.Any())
                type = gns.First().TypeArgumentList.Arguments.First();

            return BuildTypeName(type, property);
        }
    }
}
