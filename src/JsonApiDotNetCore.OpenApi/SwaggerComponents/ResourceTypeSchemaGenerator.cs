using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceTypeSchemaGenerator
{
    private const string ResourceTypeSchemaIdTemplate = "[ResourceName] Resource Type";
    private readonly IJsonApiOptions _options;

    public ResourceTypeSchemaGenerator(IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(options);

        _options = options;
    }

    public OpenApiSchema Get(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(schemaRepository);

        if (schemaRepository.TryLookupByType(resourceType.ClrType, out OpenApiSchema? referenceSchema))
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

        schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(resourceType.ClrType, schemaId);

        return referenceSchema;
    }

    private string GetSchemaId(ResourceType resourceType)
    {
        string pascalCaseSchemaId = ResourceTypeSchemaIdTemplate.Replace("[ResourceName]", resourceType.PublicName.Singularize()).ToPascalCase();

        JsonNamingPolicy? namingPolicy = _options.SerializerOptions.PropertyNamingPolicy;
        return namingPolicy != null ? namingPolicy.ConvertName(pascalCaseSchemaId) : pascalCaseSchemaId;
    }
}
