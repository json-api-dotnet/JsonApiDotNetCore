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
    private readonly Dictionary<Type, OpenApiSchema> _resourceClrTypeSchemaCache = [];

    public ResourceTypeSchemaGenerator(ISchemaRepositoryAccessor schemaRepositoryAccessor, IResourceGraph resourceGraph, JsonNamingPolicy? namingPolicy)
    {
        ArgumentGuard.NotNull(schemaRepositoryAccessor);
        ArgumentGuard.NotNull(resourceGraph);

        _schemaRepositoryAccessor = schemaRepositoryAccessor;
        _resourceGraph = resourceGraph;
        _namingPolicy = namingPolicy;
    }

    public OpenApiSchema Get(Type resourceClrType)
    {
        ArgumentGuard.NotNull(resourceClrType);

        if (_resourceClrTypeSchemaCache.TryGetValue(resourceClrType, out OpenApiSchema? extendedReferenceSchema))
        {
            return extendedReferenceSchema;
        }

        ResourceType resourceType = _resourceGraph.GetResourceType(resourceClrType);

        var fullSchema = new OpenApiSchema
        {
            Type = "string",
            Enum = new List<IOpenApiAny>
            {
                new OpenApiString(resourceType.PublicName)
            },
            AdditionalPropertiesAllowed = false
        };

        string schemaId = GetSchemaId(resourceType);

        var referenceSchema = new OpenApiSchema
        {
            Reference = new OpenApiReference
            {
                Id = schemaId,
                Type = ReferenceType.Schema
            }
        };

        extendedReferenceSchema = new OpenApiSchema
        {
            AllOf = new List<OpenApiSchema>
            {
                referenceSchema
            }
        };

        _schemaRepositoryAccessor.Current.AddDefinition(schemaId, fullSchema);
        _resourceClrTypeSchemaCache.Add(resourceType.ClrType, extendedReferenceSchema);

        return extendedReferenceSchema;
    }

    private string GetSchemaId(ResourceType resourceType)
    {
        string pascalCaseSchemaId = ResourceTypeSchemaIdTemplate.Replace("[ResourceName]", resourceType.PublicName.Singularize()).ToPascalCase();

        return _namingPolicy != null ? _namingPolicy.ConvertName(pascalCaseSchemaId) : pascalCaseSchemaId;
    }
}
