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
    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly Dictionary<Type, OpenApiSchema> _resourceClrTypeSchemaCache = [];

    public ResourceTypeSchemaGenerator(ISchemaRepositoryAccessor schemaRepositoryAccessor, JsonNamingPolicy? namingPolicy)
    {
        ArgumentGuard.NotNull(schemaRepositoryAccessor);

        _schemaRepositoryAccessor = schemaRepositoryAccessor;
        _namingPolicy = namingPolicy;
    }

    public OpenApiSchema Get(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        if (_resourceClrTypeSchemaCache.TryGetValue(resourceType.ClrType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

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

        referenceSchema = new OpenApiSchema
        {
            Reference = new OpenApiReference
            {
                Id = schemaId,
                Type = ReferenceType.Schema
            }
        };

        _schemaRepositoryAccessor.Current.AddDefinition(schemaId, fullSchema);
        _resourceClrTypeSchemaCache.Add(resourceType.ClrType, referenceSchema);

        return referenceSchema;
    }

    private string GetSchemaId(ResourceType resourceType)
    {
        string pascalCaseSchemaId = ResourceTypeSchemaIdTemplate.Replace("[ResourceName]", resourceType.PublicName.Singularize()).ToPascalCase();

        return _namingPolicy != null ? _namingPolicy.ConvertName(pascalCaseSchemaId) : pascalCaseSchemaId;
    }
}
