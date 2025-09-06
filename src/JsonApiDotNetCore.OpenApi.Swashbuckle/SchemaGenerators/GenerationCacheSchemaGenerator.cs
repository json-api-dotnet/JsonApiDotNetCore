using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;
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

        var hasAtomicOperationsEndpoint = (OpenApiBoolean)fullSchema.Properties[HasAtomicOperationsEndpointPropertyName].Default;
        return hasAtomicOperationsEndpoint.Value;
    }

    private OpenApiSchema GenerateFullSchema(SchemaRepository schemaRepository)
    {
        if (schemaRepository.Schemas.TryGetValue(SchemaId, out OpenApiSchema? fullSchema))
        {
            return fullSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this);

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
