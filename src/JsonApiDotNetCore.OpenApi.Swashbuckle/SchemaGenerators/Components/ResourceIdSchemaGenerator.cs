using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class ResourceIdSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly IControllerResourceMapping _controllerResourceMapping;

    public ResourceIdSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IControllerResourceMapping controllerResourceMapping)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _controllerResourceMapping = controllerResourceMapping;
    }

    public OpenApiSchema GenerateSchema(ParameterInfo parameter, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        Type? controllerType = parameter.Member.ReflectedType;
        ConsistencyGuard.ThrowIf(controllerType == null);

        ResourceType? resourceType = _controllerResourceMapping.GetResourceTypeForController(controllerType);
        ConsistencyGuard.ThrowIf(resourceType == null);

        return GenerateSchema(resourceType, schemaRepository);
    }

    public OpenApiSchema GenerateSchema(ResourceType resourceType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        OpenApiSchema idSchema = _defaultSchemaGenerator.GenerateSchema(resourceType.IdentityClrType, schemaRepository);
        ConsistencyGuard.ThrowIf(idSchema.Reference != null);

        idSchema.Type = "string";

        var hideIdTypeAttribute = resourceType.ClrType.GetCustomAttribute<HideResourceIdTypeInOpenApiAttribute>();

        if (hideIdTypeAttribute != null)
        {
            idSchema.Format = null;
        }
        else if (resourceType.IdentityClrType != typeof(string))
        {
            // When using string IDs, it's discouraged (but possible) to use an empty string as primary key value, because
            // some things won't work: get-by-id, update and delete resource are impossible, and rendered links are unusable.
            // For other ID types, provide the length constraint as a fallback in case the type hint isn't recognized.
            idSchema.MinLength = 1;
        }

        return idSchema;
    }
}
