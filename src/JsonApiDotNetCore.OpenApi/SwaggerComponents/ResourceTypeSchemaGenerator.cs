using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceTypeSchemaGenerator
{
    private const string ResourceTypeSchemaIdTemplate = "[ResourceName] Resource Type";
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
    private readonly IResourceGraph _resourceGraph;
    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly Dictionary<Type, OpenApiSchema> _resourceClrTypeSchemaCache = new();

    public ResourceTypeSchemaGenerator(ISchemaRepositoryAccessor schemaRepositoryAccessor, IResourceGraph resourceGraph, JsonNamingPolicy? namingPolicy)
    {
        ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

        _schemaRepositoryAccessor = schemaRepositoryAccessor;
        _resourceGraph = resourceGraph;
        _namingPolicy = namingPolicy;
    }

    public OpenApiSchema Get(Type resourceClrType)
    {
        ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));

        if (_resourceClrTypeSchemaCache.TryGetValue(resourceClrType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);

        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = new List<IOpenApiAny>
            {
                new OpenApiString(resourceType.PublicName)
            }
        };

        string schemaId = GetSchemaId(resourceType);

        referenceSchema = new OpenApiSchema
        {
            Reference = new OpenApiReference
            {
                Id = schemaId,
                Type = ReferenceType.Schema
            }
        };

        _schemaRepositoryAccessor.Current.AddDefinition(referenceSchema.Reference.Id, fullSchema);
        _resourceClrTypeSchemaCache.Add(resourceType.ClrType, referenceSchema);

        return referenceSchema;
    }

    private string GetSchemaId(ResourceType resourceType)
    {
        string pascalCaseSchemaId = ResourceTypeSchemaIdTemplate.Replace("[ResourceName]", resourceType.PublicName.Singularize()).ToPascalCase();

        return _namingPolicy != null ? _namingPolicy.ConvertName(pascalCaseSchemaId) : pascalCaseSchemaId;
    }
}
