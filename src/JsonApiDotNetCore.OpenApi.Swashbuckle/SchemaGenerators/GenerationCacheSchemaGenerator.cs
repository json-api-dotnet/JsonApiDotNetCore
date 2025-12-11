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

        OpenApiSchema inlineSchema = GenerateInlineSchema(schemaRepository);

        return inlineSchema.Properties != null &&
            inlineSchema.Properties.TryGetValue(HasAtomicOperationsEndpointPropertyName, out IOpenApiSchema? propertyValue) && (bool)propertyValue.Default!;
    }

    private OpenApiSchema GenerateInlineSchema(SchemaRepository schemaRepository)
    {
        if (schemaRepository.Schemas.TryGetValue(SchemaId, out IOpenApiSchema? existingSchema))
        {
            return existingSchema.AsInlineSchema();
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this);

        bool hasAtomicOperationsEndpoint = EvaluateHasAtomicOperationsEndpoint();

        var inlineSchema = new OpenApiSchema
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

        schemaRepository.AddDefinition(SchemaId, inlineSchema);

        traceScope.TraceSucceeded(SchemaId);
        return inlineSchema;
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
