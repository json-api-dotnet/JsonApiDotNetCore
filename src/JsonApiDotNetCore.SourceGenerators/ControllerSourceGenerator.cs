using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace JsonApiDotNetCore.SourceGenerators
{
    [Generator(LanguageNames.CSharp)]
    public sealed class ControllerSourceGenerator : ISourceGenerator
    {
        private const string Category = "JsonApiDotNetCore";

        private static readonly DiagnosticDescriptor MissingInterfaceWarning = new DiagnosticDescriptor("JADNC001",
            "Resource type does not implement IIdentifiable<TId>",
            "Type '{0}' must implement IIdentifiable<TId> when using ResourceAttribute to auto-generate ASP.NET controllers", Category,
            DiagnosticSeverity.Warning, true);

        private static readonly DiagnosticDescriptor MissingIndentInTableError = new DiagnosticDescriptor("JADNC900",
            "Internal error: Insufficient entries in IndentTable", "Internal error: Missing entry in IndentTable for depth {0}", Category,
            DiagnosticSeverity.Warning, true);

        // PERF: Heap-allocate the delegate only once, instead of per compilation.
        private static readonly SyntaxReceiverCreator CreateSyntaxReceiver = () => new TypeWithAttributeSyntaxReceiver();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(CreateSyntaxReceiver);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (TypeWithAttributeSyntaxReceiver)context.SyntaxReceiver;

            if (receiver == null)
            {
                return;
            }

            INamedTypeSymbol resourceAttributeType = context.Compilation.GetTypeByMetadataName("JsonApiDotNetCore.Resources.Annotations.ResourceAttribute");
            INamedTypeSymbol identifiableOpenInterface = context.Compilation.GetTypeByMetadataName("JsonApiDotNetCore.Resources.IIdentifiable`1");
            INamedTypeSymbol loggerFactoryInterface = context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILoggerFactory");

            if (resourceAttributeType == null || identifiableOpenInterface == null || loggerFactoryInterface == null)
            {
                return;
            }

            var controllerNamesInUse = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var writer = new SourceCodeWriter(context, MissingIndentInTableError);

            foreach (TypeDeclarationSyntax typeDeclarationSyntax in receiver.TypeDeclarations)
            {
                // PERF: Note that our code runs on every keystroke in the IDE, which makes it critical to provide near-realtime performance.
                // This means keeping an eye on memory allocations and bailing out early when compilations are cancelled while the user is still typing.
                context.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel semanticModel = context.Compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
                INamedTypeSymbol resourceType = semanticModel.GetDeclaredSymbol(typeDeclarationSyntax, context.CancellationToken);

                if (resourceType == null)
                {
                    continue;
                }

                AttributeData resourceAttributeData = FirstOrDefault(resourceType.GetAttributes(), resourceAttributeType,
                    (data, type) => SymbolEqualityComparer.Default.Equals(data.AttributeClass, type));

                if (resourceAttributeData == null)
                {
                    continue;
                }

                TypedConstant endpointsArgument = resourceAttributeData.NamedArguments.FirstOrDefault(pair => pair.Key == "GenerateControllerEndpoints").Value;

                if (endpointsArgument.Value != null && (JsonApiEndpointsCopy)endpointsArgument.Value == JsonApiEndpointsCopy.None)
                {
                    continue;
                }

                TypedConstant controllerNamespaceArgument =
                    resourceAttributeData.NamedArguments.FirstOrDefault(pair => pair.Key == "ControllerNamespace").Value;

                string controllerNamespace = GetControllerNamespace(controllerNamespaceArgument, resourceType);

                INamedTypeSymbol identifiableClosedInterface = FirstOrDefault(resourceType.AllInterfaces, identifiableOpenInterface,
                    (@interface, openInterface) => @interface.IsGenericType &&
                        SymbolEqualityComparer.Default.Equals(@interface.ConstructedFrom, openInterface));

                if (identifiableClosedInterface == null)
                {
                    var diagnostic = Diagnostic.Create(MissingInterfaceWarning, typeDeclarationSyntax.GetLocation(), resourceType.Name);
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }

                ITypeSymbol idType = identifiableClosedInterface.TypeArguments[0];
                string controllerName = $"{resourceType.Name.Pluralize()}Controller";
                JsonApiEndpointsCopy endpointsToGenerate = (JsonApiEndpointsCopy?)(int?)endpointsArgument.Value ?? JsonApiEndpointsCopy.All;

                string sourceCode = writer.Write(resourceType, idType, endpointsToGenerate, controllerNamespace, controllerName, loggerFactoryInterface);
                SourceText sourceText = SourceText.From(sourceCode, Encoding.UTF8);

                string fileName = GetUniqueFileName(controllerName, controllerNamesInUse);
                context.AddSource(fileName, sourceText);
            }
        }

        private static TElement FirstOrDefault<TElement, TContext>(ImmutableArray<TElement> source, TContext context, Func<TElement, TContext, bool> predicate)
        {
            // PERF: Using this method enables to avoid allocating a closure in the passed lambda expression.
            // See https://www.jetbrains.com/help/resharper/2021.2/Fixing_Issues_Found_by_DPA.html#closures-in-lambda-expressions.

            foreach (TElement element in source)
            {
                if (predicate(element, context))
                {
                    return element;
                }
            }

            return default;
        }

        private static string GetControllerNamespace(TypedConstant controllerNamespaceArgument, INamedTypeSymbol resourceType)
        {
            if (!controllerNamespaceArgument.IsNull)
            {
                return (string)controllerNamespaceArgument.Value;
            }

            if (resourceType.ContainingNamespace.IsGlobalNamespace)
            {
                return null;
            }

            if (resourceType.ContainingNamespace.ContainingNamespace.IsGlobalNamespace)
            {
                return "Controllers";
            }

            return $"{resourceType.ContainingNamespace.ContainingNamespace}.Controllers";
        }

        private static string GetUniqueFileName(string controllerName, IDictionary<string, int> controllerNamesInUse)
        {
            // We emit unique file names to prevent a failure in the source generator, but also because our test suite
            // may contain two resources with the same class name in different namespaces. That works, as long as only
            // one of its controllers gets registered.

            if (controllerNamesInUse.TryGetValue(controllerName, out int lastIndex))
            {
                lastIndex++;
                controllerNamesInUse[controllerName] = lastIndex;

                return $"{controllerName}{lastIndex}.g.cs";
            }

            controllerNamesInUse[controllerName] = 1;
            return $"{controllerName}.g.cs";
        }
    }
}
