using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi;
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

    public GenerationCacheSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, IActionDescriptorCollectionProvider defaultProvider)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(defaultProvider);

        _schemaGenerationTracer = schemaGenerationTracer;
        _defaultProvider = defaultProvider;
    }

    public bool HasAtomicOperationsEndpoint(SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);

        OpenApiSchema fullSchema = GenerateFullSchema(schemaRepository);

        return fullSchema.Properties != null && (bool)fullSchema.Properties[HasAtomicOperationsEndpointPropertyName].Default!;
    }

    private OpenApiSchema GenerateFullSchema(SchemaRepository schemaRepository)
    {
        if (schemaRepository.Schemas.TryGetValue(SchemaId, out IOpenApiSchema? existingFullSchema))
        {
            return (OpenApiSchema)existingFullSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this);

        bool hasAtomicOperationsEndpoint = EvaluateHasAtomicOperationsEndpoint();

        var fullSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [HasAtomicOperationsEndpointPropertyName] = new OpenApiSchema
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
        IEnumerable<ActionDescriptor> descriptors = _defaultProvider.ActionDescriptors.Items.Where(JsonApiActionDescriptorCollectionProvider.IsVisibleEndpoint);

        foreach (ActionDescriptor descriptor in descriptors)
        {
            var actionMethod = JsonApiActionMethod.TryCreate(descriptor);

            if (actionMethod is OperationsActionMethod)
            {
                return true;
            }
        }

        return false;
    }
}
