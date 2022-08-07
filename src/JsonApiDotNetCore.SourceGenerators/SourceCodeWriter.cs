using System.Text;
using Microsoft.CodeAnalysis;

namespace JsonApiDotNetCore.SourceGenerators;

/// <summary>
/// Writes the source code for an ASP.NET controller for a JSON:API resource.
/// </summary>
internal sealed class SourceCodeWriter
{
    private const int SpacesPerIndent = 4;

    private static readonly IDictionary<int, string> IndentTable = new Dictionary<int, string>
    {
        [0] = string.Empty,
        [1] = new(' ', 1 * SpacesPerIndent),
        [2] = new(' ', 2 * SpacesPerIndent),
        [3] = new(' ', 3 * SpacesPerIndent)
    };

    private static readonly IDictionary<JsonApiEndpointsCopy, (string ServiceName, string ParameterName)> AggregateEndpointToServiceNameMap =
        new Dictionary<JsonApiEndpointsCopy, (string, string)>
        {
            [JsonApiEndpointsCopy.All] = ("IResourceService", "resourceService"),
            [JsonApiEndpointsCopy.Query] = ("IResourceQueryService", "queryService"),
            [JsonApiEndpointsCopy.Command] = ("IResourceCommandService", "commandService")
        };

    private static readonly IDictionary<JsonApiEndpointsCopy, (string ServiceName, string ParameterName)> EndpointToServiceNameMap =
        new Dictionary<JsonApiEndpointsCopy, (string, string)>
        {
            [JsonApiEndpointsCopy.GetCollection] = ("IGetAllService", "getAll"),
            [JsonApiEndpointsCopy.GetSingle] = ("IGetByIdService", "getById"),
            [JsonApiEndpointsCopy.GetSecondary] = ("IGetSecondaryService", "getSecondary"),
            [JsonApiEndpointsCopy.GetRelationship] = ("IGetRelationshipService", "getRelationship"),
            [JsonApiEndpointsCopy.Post] = ("ICreateService", "create"),
            [JsonApiEndpointsCopy.PostRelationship] = ("IAddToRelationshipService", "addToRelationship"),
            [JsonApiEndpointsCopy.Patch] = ("IUpdateService", "update"),
            [JsonApiEndpointsCopy.PatchRelationship] = ("ISetRelationshipService", "setRelationship"),
            [JsonApiEndpointsCopy.Delete] = ("IDeleteService", "delete"),
            [JsonApiEndpointsCopy.DeleteRelationship] = ("IRemoveFromRelationshipService", "removeFromRelationship")
        };

    private readonly GeneratorExecutionContext _context;
    private readonly DiagnosticDescriptor _missingIndentInTableErrorDescriptor;

    private readonly StringBuilder _sourceBuilder = new();
    private int _depth;

    public SourceCodeWriter(GeneratorExecutionContext context, DiagnosticDescriptor missingIndentInTableErrorDescriptor)
    {
        _context = context;
        _missingIndentInTableErrorDescriptor = missingIndentInTableErrorDescriptor;
    }

    public string Write(INamedTypeSymbol resourceType, ITypeSymbol idType, JsonApiEndpointsCopy endpointsToGenerate, string? controllerNamespace,
        string controllerName, INamedTypeSymbol loggerFactoryInterface)
    {
        _sourceBuilder.Clear();
        _depth = 0;

        if (idType.IsReferenceType && idType.NullableAnnotation == NullableAnnotation.Annotated)
        {
            WriteNullableEnable();
        }

        WriteNamespaceImports(loggerFactoryInterface, resourceType);

        if (controllerNamespace != null)
        {
            WriteNamespaceDeclaration(controllerNamespace);
        }

        WriteOpenClassDeclaration(controllerName, endpointsToGenerate, resourceType, idType);
        _depth++;

        WriteConstructor(controllerName, loggerFactoryInterface, endpointsToGenerate, resourceType, idType);

        _depth--;
        WriteCloseCurly();

        return _sourceBuilder.ToString();
    }

    private void WriteNullableEnable()
    {
        _sourceBuilder.AppendLine("#nullable enable");
        _sourceBuilder.AppendLine();
    }

    private void WriteNamespaceImports(INamedTypeSymbol loggerFactoryInterface, INamedTypeSymbol resourceType)
    {
        _sourceBuilder.AppendLine($@"using {loggerFactoryInterface.ContainingNamespace};");

        _sourceBuilder.AppendLine("using JsonApiDotNetCore.Configuration;");
        _sourceBuilder.AppendLine("using JsonApiDotNetCore.Controllers;");
        _sourceBuilder.AppendLine("using JsonApiDotNetCore.Services;");

        if (!resourceType.ContainingNamespace.IsGlobalNamespace)
        {
            _sourceBuilder.AppendLine($"using {resourceType.ContainingNamespace};");
        }

        _sourceBuilder.AppendLine();
    }

    private void WriteNamespaceDeclaration(string controllerNamespace)
    {
        _sourceBuilder.AppendLine($"namespace {controllerNamespace};");
        _sourceBuilder.AppendLine();
    }

    private void WriteOpenClassDeclaration(string controllerName, JsonApiEndpointsCopy endpointsToGenerate, INamedTypeSymbol resourceType, ITypeSymbol idType)
    {
        string baseClassName = GetControllerBaseClassName(endpointsToGenerate);

        WriteIndent();
        _sourceBuilder.AppendLine($@"public sealed partial class {controllerName} : {baseClassName}<{resourceType.Name}, {idType}>");

        WriteOpenCurly();
    }

    private static string GetControllerBaseClassName(JsonApiEndpointsCopy endpointsToGenerate)
    {
        switch (endpointsToGenerate)
        {
            case JsonApiEndpointsCopy.Query:
            {
                return "JsonApiQueryController";
            }
            case JsonApiEndpointsCopy.Command:
            {
                return "JsonApiCommandController";
            }
            default:
            {
                return "JsonApiController";
            }
        }
    }

    private void WriteConstructor(string controllerName, INamedTypeSymbol loggerFactoryInterface, JsonApiEndpointsCopy endpointsToGenerate,
        INamedTypeSymbol resourceType, ITypeSymbol idType)
    {
        string loggerName = loggerFactoryInterface.Name;

        WriteIndent();
        _sourceBuilder.AppendLine($"public {controllerName}(IJsonApiOptions options, IResourceGraph resourceGraph, {loggerName} loggerFactory,");

        _depth++;

        if (AggregateEndpointToServiceNameMap.TryGetValue(endpointsToGenerate, out (string ServiceName, string ParameterName) value))
        {
            WriteParameterListForShortConstructor(value.ServiceName, value.ParameterName, resourceType, idType);
        }
        else
        {
            WriteParameterListForLongConstructor(endpointsToGenerate, resourceType, idType);
        }

        _depth--;

        WriteOpenCurly();
        WriteCloseCurly();
    }

    private void WriteParameterListForShortConstructor(string serviceName, string parameterName, INamedTypeSymbol resourceType, ITypeSymbol idType)
    {
        WriteIndent();
        _sourceBuilder.AppendLine($"{serviceName}<{resourceType.Name}, {idType}> {parameterName})");

        WriteIndent();
        _sourceBuilder.AppendLine($": base(options, resourceGraph, loggerFactory, {parameterName})");
    }

    private void WriteParameterListForLongConstructor(JsonApiEndpointsCopy endpointsToGenerate, INamedTypeSymbol resourceType, ITypeSymbol idType)
    {
        bool isFirstEntry = true;

        foreach (KeyValuePair<JsonApiEndpointsCopy, (string ServiceName, string ParameterName)> entry in EndpointToServiceNameMap)
        {
            if ((endpointsToGenerate & entry.Key) == entry.Key)
            {
                if (isFirstEntry)
                {
                    isFirstEntry = false;
                }
                else
                {
                    _sourceBuilder.AppendLine(Tokens.Comma);
                }

                WriteIndent();
                _sourceBuilder.Append($"{entry.Value.ServiceName}<{resourceType.Name}, {idType}> {entry.Value.ParameterName}");
            }
        }

        _sourceBuilder.AppendLine(Tokens.CloseParen);

        WriteIndent();
        _sourceBuilder.AppendLine(": base(options, resourceGraph, loggerFactory,");

        isFirstEntry = true;
        _depth++;

        foreach (KeyValuePair<JsonApiEndpointsCopy, (string ServiceName, string ParameterName)> entry in EndpointToServiceNameMap)
        {
            if ((endpointsToGenerate & entry.Key) == entry.Key)
            {
                if (isFirstEntry)
                {
                    isFirstEntry = false;
                }
                else
                {
                    _sourceBuilder.AppendLine(Tokens.Comma);
                }

                WriteIndent();
                _sourceBuilder.Append($"{entry.Value.ParameterName}: {entry.Value.ParameterName}");
            }
        }

        _sourceBuilder.AppendLine(Tokens.CloseParen);
        _depth--;
    }

    private void WriteOpenCurly()
    {
        WriteIndent();
        _sourceBuilder.AppendLine(Tokens.OpenCurly);
    }

    private void WriteCloseCurly()
    {
        WriteIndent();
        _sourceBuilder.AppendLine(Tokens.CloseCurly);
    }

    private void WriteIndent()
    {
        // PERF: Reuse pre-calculated indents instead of allocating a new string each time.
        if (!IndentTable.TryGetValue(_depth, out string? indent))
        {
            var diagnostic = Diagnostic.Create(_missingIndentInTableErrorDescriptor, Location.None, _depth.ToString());
            _context.ReportDiagnostic(diagnostic);

            indent = new string(' ', _depth * SpacesPerIndent);
        }

        _sourceBuilder.Append(indent);
    }

#pragma warning disable AV1008 // Class should not be static
    private static class Tokens
    {
        public const string OpenCurly = "{";
        public const string CloseCurly = "}";
        public const string CloseParen = ")";
        public const string Comma = ",";
    }
#pragma warning restore AV1008 // Class should not be static
}
