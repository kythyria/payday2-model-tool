using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ModelToolSourceGenerator
{
    [Generator]
    public class ModelLoaderGenerator : ISourceGenerator
    {
        private SwitchSectionSyntax MakeConstructionSection(SwitchLabelSyntax label, TypeSyntax type) => SwitchSection()
            .WithLabels(new SyntaxList<SwitchLabelSyntax>(label))
            .AddStatements(
                ReturnStatement(
                    ObjectCreationExpression(type)
                        .AddArgumentListArguments(
                            Argument(ParseExpression("br")),
                            Argument(ParseExpression("sh")))));

        public void Execute(SourceGeneratorContext context)
        {
            var sectioninterface = context.Compilation.GetSymbolsWithName(i => i.Contains("ISection")).FirstOrDefault();
            var sectionattribute = context.Compilation.GetSymbolsWithName(i => i.Contains("SectionIdAttribute")).FirstOrDefault();

            var classes = (context.SyntaxReceiver as ClassFindingSyntaxReceiver).classes;
            var symbols = new List<INamedTypeSymbol>();
            var switchblocks = new SyntaxList<SwitchSectionSyntax>();
            foreach(var c in classes)
            {
                var cs = context.Compilation.GetSemanticModel(c.SyntaxTree);
                var si = cs.GetDeclaredSymbol(c);
                if (si is INamedTypeSymbol sym && sym.AllInterfaces.Contains(sectioninterface))
                {
                    var idatt = sym.GetAttributes().Where(i => i.AttributeClass.Equals(sectionattribute)).FirstOrDefault();
                    if (idatt != null)
                    {
                        symbols.Add(sym);

                        var tag = idatt.ConstructorArguments[0].Value;
                        var literal = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((uint)tag));
                        switchblocks = switchblocks.Add(MakeConstructionSection(CaseSwitchLabel(literal), ParseTypeName(sym.Name)));
                    }
                }
            }
            switchblocks = switchblocks.Add(MakeConstructionSection(DefaultSwitchLabel(), ParseTypeName("Unknown")));

            var readsectionmethod = MethodDeclaration(ParseTypeName("ISection"), "ReadSection").AddModifiers(Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("br")).WithType(ParseTypeName("System.IO.BinaryReader")),
                    Parameter(Identifier("sh")).WithType(ParseTypeName("SectionHeader")))
                .WithBody(Block(
                    SwitchStatement(ParseExpression("sh.type"), switchblocks)));

            var cu = CompilationUnit()
                .AddUsings(UsingDirective(ParseName("PD2ModelParser.Sections")))
                .AddMembers(NamespaceDeclaration(ParseName("PD2ModelParser"))
                    .AddMembers(ClassDeclaration("ModelReader")
                        .AddModifiers(Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                        .AddMembers(readsectionmethod)
                        .AddMembers(MethodDeclaration(ParseTypeName("void"), "SayHello").AddModifiers(Token(SyntaxKind.PublicKeyword),Token(SyntaxKind.StaticKeyword)).WithBody(Block()))));

            var sourceBuilder = new StringBuilder($"{cu.NormalizeWhitespace().GetText()}");
            context.AddSource("modelLoaderGenerator", /*cu.NormalizeWhitespace().GetText(Encoding.UTF8) */ SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ClassFindingSyntaxReceiver());
        }
    }

    class SectionFindingVisitor : SymbolVisitor
    {
        public List<INamedTypeSymbol> Symbols { get; set; } = new List<INamedTypeSymbol>();
        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            Symbols.Add(symbol);
            base.VisitNamedType(symbol);
        }
    }

    class ClassFindingSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> classes { get; private set; } = new List<ClassDeclarationSyntax>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode is ClassDeclarationSyntax cds)
            {
                classes.Add(cds);
            }
        }
    }
}
