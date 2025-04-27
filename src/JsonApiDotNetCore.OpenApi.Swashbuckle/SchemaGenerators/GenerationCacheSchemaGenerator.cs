using System.Reflection;
using System.Text.Json.Nodes;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators;

/// <summary>
/// Provides access to cached state, which is stored in a temporary schema in <see cref="SchemaRepository" /> during schema generation.
/// </summary>
internal sealed class GenerationCacheSchemaGenerator
{
    private const string HasAtomicOperationsEndpointPropertyName = "HasAtomicOperationsEndpoint";
    public const string SchemaId = "__JsonApiSchemaGenerationCache__";

    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly IActionDescriptorCollectionProvider _defaultProvider;
    private readonly JsonApiEndpointMetadataProvider _jsonApiEndpointMetadataProvider;

    public GenerationCacheSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, IActionDescriptorCollectionProvider defaultProvider,
        JsonApiEndpointMetadataProvider jsonApiEndpointMetadataProvider)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(defaultProvider);
        ArgumentNullException.ThrowIfNull(jsonApiEndpointMetadataProvider);

        _schemaGenerationTracer = schemaGenerationTracer;
        _defaultProvider = defaultProvider;
        _jsonApiEndpointMetadataProvider = jsonApiEndpointMetadataProvider;
    }

    public bool HasAtomicOperationsEndpoint(SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        var fullSchema = GenerateFullSchema(schemaRepository);

        return (bool)fullSchema.Properties[HasAtomicOperationsEndpointPropertyName].Default;
    }

    private OpenApiSchema GenerateFullSchema(SchemaRepository schemaRepository)
    {
        if (schemaRepository.Schemas.TryGetValue(SchemaId, out var schema))
        {
            return (OpenApiSchema)schema;
        }

        using var traceScope = _schemaGenerationTracer.TraceStart(this);

        var hasAtomicOperationsEndpoint = EvaluateHasAtomicOperationsEndpoint();

        var fullSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [HasAtomicOperationsEndpointPropertyName] = new OpenApiSchema()
                {
                    Type = JsonSchemaType.Boolean,
                    Default = hasAtomicOperationsEndpoint
                }
            }
        };

        schemaRepository.AddDefinition(SchemaId, fullSchema);

        traceScope.TraceSucceeded(SchemaId);
        return fullSchema;
    }

    private bool EvaluateHasAtomicOperationsEndpoint()
    {
        var actionDescriptors =
            _defaultProvider.ActionDescriptors.Items.Where(JsonApiActionDescriptorCollectionProvider.IsVisibleJsonApiEndpoint);

        foreach (var actionDescriptor in actionDescriptors)
        {
            var actionMethod = actionDescriptor.GetActionMethod();
            var endpointMetadataContainer = _jsonApiEndpointMetadataProvider.Get(actionMethod);

            if (endpointMetadataContainer.RequestMetadata is AtomicOperationsRequestMetadata)
            {
                return true;
            }
        }

        return false;
    }
}
