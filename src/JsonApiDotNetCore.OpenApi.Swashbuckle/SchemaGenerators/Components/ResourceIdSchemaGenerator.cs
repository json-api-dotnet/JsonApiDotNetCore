using JsonApiDotNetCore.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class ResourceIdSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;

    public ResourceIdSchemaGenerator(SchemaGenerator defaultSchemaGenerator)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);

        _defaultSchemaGenerator = defaultSchemaGenerator;
    }

    public OpenApiSchema GenerateSchema(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        return GenerateSchema(resourceType.IdentityClrType, schemaRepository);
    }

    public OpenApiSchema GenerateSchema(Type resourceIdClrType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(resourceIdClrType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        OpenApiSchema idSchema = _defaultSchemaGenerator.GenerateSchema(resourceIdClrType, schemaRepository);
        idSchema.Type = "string";

        if (resourceIdClrType != typeof(string))
        {
            // When using string IDs, it's discouraged (but possible) to use an empty string as primary key value, because
            // some things won't work: get-by-id, update and delete resource are impossible, and rendered links are unusable.
            // For other ID types, provide the length constraint as a fallback in case the type hint isn't recognized.
            idSchema.MinLength = 1;
        }

        return idSchema;
    }
}
