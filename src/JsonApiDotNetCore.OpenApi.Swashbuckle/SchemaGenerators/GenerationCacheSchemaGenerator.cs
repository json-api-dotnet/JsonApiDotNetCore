using System.Reflection;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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

        OpenApiSchema referenceSchema = GenerateFullSchema(schemaRepository);

        var hasAtomicOperationsEndpoint = (OpenApiBoolean)referenceSchema.Properties[HasAtomicOperationsEndpointPropertyName].Default;
        return hasAtomicOperationsEndpoint.Value;
    }

    private OpenApiSchema GenerateFullSchema(SchemaRepository schemaRepository)
    {
        if (schemaRepository.Schemas.TryGetValue(SchemaId, out OpenApiSchema? fullSchema))
        {
            return fullSchema;
        }

        using IDisposable traceScope = _schemaGenerationTracer.TraceStart(this, SchemaId);

        bool hasAtomicOperationsEndpoint = EvaluateHasAtomicOperationsEndpoint();

        fullSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [HasAtomicOperationsEndpointPropertyName] = new()
                {
                    Type = "boolean",
                    Default = new OpenApiBoolean(hasAtomicOperationsEndpoint)
                }
            }
        };

        schemaRepository.AddDefinition(SchemaId, fullSchema);
        return schemaRepository.Schemas[SchemaId];
    }

    private bool EvaluateHasAtomicOperationsEndpoint()
    {
        IEnumerable<ActionDescriptor> actionDescriptors =
            _defaultProvider.ActionDescriptors.Items.Where(JsonApiActionDescriptorCollectionProvider.IsVisibleJsonApiEndpoint);

        foreach (ActionDescriptor actionDescriptor in actionDescriptors)
        {
            MethodInfo actionMethod = actionDescriptor.GetActionMethod();
            JsonApiEndpointMetadataContainer endpointMetadataContainer = _jsonApiEndpointMetadataProvider.Get(actionMethod);

            if (endpointMetadataContainer.RequestMetadata is AtomicOperationsRequestMetadata)
            {
                return true;
            }
        }

        return false;
    }
}
