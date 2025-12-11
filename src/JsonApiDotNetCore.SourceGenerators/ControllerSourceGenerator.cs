using System.Collections.Immutable;
using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// To debug in Visual Studio (requires v17.8 or higher):
// - Set JsonApiDotNetCore.SourceGenerators as startup project
// - Add a breakpoint at the start of the Initialize method
// - Optional: change targetProject in Properties\launchSettings.json
// - Press F5

#pragma warning disable format

namespace JsonApiDotNetCore.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class ControllerSourceGenerator : IIncrementalGenerator
{
    private const string ResourceAttributeName = "ResourceAttribute";
    private const string ResourceAttributeFullName = $"JsonApiDotNetCore.Resources.Annotations.{ResourceAttributeName}";
    private const string IdentifiableInterfaceName = "IIdentifiable";
    private const string IdentifiableOpenGenericInterfaceName = "JsonApiDotNetCore.Resources.IIdentifiable<TId>";

    private const string Category = "JsonApiDotNetCore";

    private static readonly DiagnosticDescriptor MissingInterfaceWarning = new("JADNC001", "Resource type does not implement IIdentifiable<TId>",
        "Type '{0}' must implement IIdentifiable<TId> when using ResourceAttribute to auto-generate ASP.NET controllers", Category, DiagnosticSeverity.Warning,
        true);

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
    private static readonly string LineBreak = Environment.NewLine;
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

    public bool RaiseErrorForTesting { get; init; }

    // Based on perf tips at https://andrewlock.net/creating-a-source-generator-part-9-avoiding-performance-pitfalls-in-incremental-generators/.

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // @formatter:keep_existing_linebreaks true

        IncrementalValuesProvider<SemanticResult?> nullableResultsProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(ResourceAttributeFullName,
                static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax,
                static (generatorContext, _) => TryGetSemanticTarget(generatorContext))
            .WithTrackingName(TrackingNames.GetSemanticTarget);

        IncrementalValuesProvider<SemanticResult> resultsProvider = nullableResultsProvider
            .Where(static result => result is not null)
            .Select(static (result, _) => result!.Value)
            .WithTrackingName(TrackingNames.FilterNulls);

        IncrementalValuesProvider<MissingInterfaceDiagnostic> diagnosticsProvider = resultsProvider
            .Where(static result => result is { Diagnostic: not null })
            .Select(static (result, _) => result.Diagnostic!.Value)
            .WithTrackingName(TrackingNames.FilterDiagnostics);

        context.RegisterSourceOutput(diagnosticsProvider,
            static (context, diagnosticInfo) => ReportDiagnostic(diagnosticInfo, context));

        IncrementalValuesProvider<CoreControllerInfo> coreControllersProvider = resultsProvider
            .Where(static result => result is { CoreController: not null })
            .Select(static (result, _) => result.CoreController!.Value)
            .WithTrackingName(TrackingNames.FilterCoreControllers);

        IncrementalValuesProvider<FullControllerInfo> fullControllersProvider = coreControllersProvider
            .Select(static (coreController, _) => EnrichController(coreController))
            .WithTrackingName(TrackingNames.EnrichCoreControllers);

        // Must ensure unique file names, see https://github.com/dotnet/roslyn/discussions/60272#discussioncomment-6053422.
        IncrementalValueProvider<ImmutableDictionary<TypeInfo, string>> renameMappingProvider = fullControllersProvider
            .Select(static (controller, _) => (controller.ControllerType, controller.HintFileName))
            .Collect()
            .Select(static (collection, _) => CreateRenameMapping(collection))
            .WithComparer(ImmutableDictionaryEqualityComparer<TypeInfo, string>.Instance)
            .WithTrackingName(TrackingNames.CreateRenameMapping);

        IncrementalValuesProvider<FullControllerInfo> uniquelyNamedControllersProvider = fullControllersProvider
            .Combine(renameMappingProvider)
            .Select(static (tuple, _) => ApplyRenameMapping(tuple.Left, tuple.Right))
            .WithTrackingName(TrackingNames.ApplyRenameMapping);

        context.RegisterSourceOutput(uniquelyNamedControllersProvider,
            (productionContext, controller) => GenerateCode(productionContext, in controller));

        // @formatter:keep_existing_linebreaks restore
    }

    private static SemanticResult? TryGetSemanticTarget(GeneratorAttributeSyntaxContext generatorContext)
    {
        if (generatorContext.TargetNode is not TypeDeclarationSyntax resourceTypeSyntax)
        {
            return null;
        }

        if (generatorContext.TargetSymbol is not INamedTypeSymbol resourceTypeSymbol)
        {
            return null;
        }

        AttributeData? resourceAttribute = TryGetResourceAttribute(resourceTypeSymbol);

        if (resourceAttribute == null)
        {
            return null;
        }

        ITypeSymbol? idTypeSymbol = TryGetIdTypeSymbol(resourceTypeSymbol);

        if (idTypeSymbol == null)
        {
            return CreateDiagnosticForMissingInterface(resourceTypeSyntax);
        }

        (JsonApiEndpointsCopy endpoints, string? controllerNamespace) = GetResourceAttributeArguments(resourceAttribute);

        if (endpoints == JsonApiEndpointsCopy.None)
        {
            return null;
        }

        controllerNamespace ??= GetControllerNamespace(resourceTypeSymbol);
        CoreControllerInfo? controllerInfo = CoreControllerInfo.TryCreate(resourceTypeSymbol, idTypeSymbol, endpoints, controllerNamespace);

        return new SemanticResult(controllerInfo, null);
    }

    private static AttributeData? TryGetResourceAttribute(INamedTypeSymbol typeSymbol)
    {
        foreach (AttributeData attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name == ResourceAttributeName && attribute.AttributeClass.ToDisplayString() == ResourceAttributeFullName)
            {
                return attribute;
            }
        }

        return null;
    }

    private static ITypeSymbol? TryGetIdTypeSymbol(INamedTypeSymbol typeSymbol)
    {
        // This may look very expensive. However, measurements indicate that when starting from syntax, followed by resolving the symbol
        // from the semantic model, it actually takes a dozen milliseconds longer to execute.

        foreach (INamedTypeSymbol interfaceSymbol in typeSymbol.AllInterfaces)
        {
            if (interfaceSymbol.IsGenericType && interfaceSymbol.Name == IdentifiableInterfaceName &&
                interfaceSymbol.ConstructedFrom.ToDisplayString() == IdentifiableOpenGenericInterfaceName)
            {
                return interfaceSymbol.TypeArguments[0];
            }
        }

        return null;
    }

    private static SemanticResult CreateDiagnosticForMissingInterface(TypeDeclarationSyntax resourceTypeSyntax)
    {
        LocationInfo? location = LocationInfo.TryCreateFrom(resourceTypeSyntax);
        return new SemanticResult(null, new MissingInterfaceDiagnostic(resourceTypeSyntax.Identifier.ValueText, location));
    }

    private static (JsonApiEndpointsCopy endpoints, string? controllerNamespace) GetResourceAttributeArguments(AttributeData attribute)
    {
        var endpoints = JsonApiEndpointsCopy.All;
        string? controllerNamespace = null;

        if (attribute.NamedArguments is { IsEmpty: false } namedArguments)
        {
            foreach ((string argumentName, TypedConstant argumentValue) in namedArguments)
            {
                switch (argumentName)
                {
                    case "GenerateControllerEndpoints":
                    {
                        if (argumentValue.Kind is TypedConstantKind.Enum && argumentValue.Value is int enumValue)
                        {
                            endpoints = (JsonApiEndpointsCopy)enumValue;
                        }

                        break;
                    }
                    case "ControllerNamespace":
                    {
                        if (argumentValue.Kind is TypedConstantKind.Primitive && argumentValue.Value is string stringValue)
                        {
                            controllerNamespace = stringValue;
                        }

                        break;
                    }
                }
            }
        }

        return (endpoints, controllerNamespace);
    }

    private static string GetControllerNamespace(INamedTypeSymbol resourceType)
    {
        INamespaceSymbol? parentNamespace = resourceType.ContainingNamespace;

        if (parentNamespace == null || parentNamespace.IsGlobalNamespace)
        {
            return string.Empty;
        }

        INamespaceSymbol? parentParentNamespace = parentNamespace.ContainingNamespace;
        return parentParentNamespace.IsGlobalNamespace ? "Controllers" : $"{parentParentNamespace}.Controllers";
    }

    private static void ReportDiagnostic(MissingInterfaceDiagnostic diagnosticInfo, SourceProductionContext context)
    {
        var location = diagnosticInfo.Location?.ToLocation();
        var diagnostic = Diagnostic.Create(MissingInterfaceWarning, location, diagnosticInfo.ResourceTypeName);
        context.ReportDiagnostic(diagnostic);
    }

    private static FullControllerInfo EnrichController(CoreControllerInfo coreController)
    {
        // Pluralize() is an expensive call.
        string controllerTypeName = $"{coreController.ResourceType.TypeName.Pluralize()}Controller";

        return FullControllerInfo.Create(coreController, controllerTypeName);
    }

    private static ImmutableDictionary<TypeInfo, string> CreateRenameMapping(ImmutableArray<(TypeInfo ControllerType, string HintFileName)> collection)
    {
        var namesInUse = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var renameMapping = ImmutableDictionary<TypeInfo, string>.Empty;

        foreach ((TypeInfo controllerType, string hintFileName) in collection.OrderBy(static element => element.ControllerType.ToString(),
            StringComparer.Ordinal))
        {
#pragma warning disable AV1532 // Loop statement contains nested loop
            // Justification: optimized for performance.
            for (int index = -1;; index++)
#pragma warning restore AV1532 // Loop statement contains nested loop
            {
                if (index == -1)
                {
                    if (namesInUse.Add(hintFileName))
                    {
                        break;
                    }
                }
                else
                {
                    string candidateName = $"{hintFileName}{index}";

                    if (namesInUse.Add(candidateName))
                    {
                        renameMapping = renameMapping.Add(controllerType, candidateName);
                        break;
                    }
                }
            }
        }

        return renameMapping;
    }

    private static FullControllerInfo ApplyRenameMapping(FullControllerInfo fullController, ImmutableDictionary<TypeInfo, string> renameMapping)
    {
        return renameMapping.TryGetValue(fullController.ControllerType, out string? replacementHintName)
            ? fullController.WithHintFileName(replacementHintName)
            : fullController;
    }

    private void GenerateCode(SourceProductionContext productionContext, in FullControllerInfo fullController)
    {
        SourceCodeWriter writer = new();
        string fileContent;

        try
        {
            if (RaiseErrorForTesting)
            {
                throw new InvalidOperationException("Test error.");
            }

            fileContent = writer.Write(in fullController);
        }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
        catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
        {
            fileContent = GetErrorText(exception, in fullController);
        }

        string hintName = $"{fullController.HintFileName}.g.cs";
        SourceText sourceText = SourceText.From(fileContent, Encoding.UTF8);
        productionContext.AddSource(hintName, sourceText);
    }

    private static string GetErrorText(Exception exception, in FullControllerInfo fullController)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"#error Unhandled exception while generating controller class for type '{fullController.CoreController.ResourceType}'.");
        builder.AppendLine();
        builder.AppendLine($"// Input: {fullController}");
        builder.AppendLine();

        foreach (string errorLine in exception.ToString().Split(LineBreak))
        {
            builder.AppendLine($"// {errorLine}");
        }

        return builder.ToString();
    }
}
