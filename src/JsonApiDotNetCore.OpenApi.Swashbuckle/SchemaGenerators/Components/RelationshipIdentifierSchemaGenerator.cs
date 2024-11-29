using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class RelationshipIdentifierSchemaGenerator
{
#if NET6_0
    private static readonly string[] RelationshipIdentifierPropertyNamesInOrder =
    [
        JsonApiPropertyName.Type,
        JsonApiPropertyName.Id,
        JsonApiPropertyName.Lid,
        JsonApiPropertyName.Relationship
    ];
#endif

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly RelationshipNameSchemaGenerator _relationshipNameSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public RelationshipIdentifierSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        ResourceIdSchemaGenerator resourceIdSchemaGenerator, RelationshipNameSchemaGenerator relationshipNameSchemaGenerator,
        JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdSchemaGenerator);
        ArgumentGuard.NotNull(relationshipNameSchemaGenerator);
        ArgumentGuard.NotNull(schemaIdSelector);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _relationshipNameSchemaGenerator = relationshipNameSchemaGenerator;
        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(relationship);
        ArgumentGuard.NotNull(schemaRepository);

        string schemaId = _schemaIdSelector.GetRelationshipIdentifierSchemaId(relationship);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Id = schemaId,
                    Type = ReferenceType.Schema
                }
            };
        }

        Type relationshipIdentifierConstructedType = typeof(RelationshipIdentifier<>).MakeGenericType(relationship.LeftType.ClrType);

        if (schemaRepository.TryLookupByType(relationshipIdentifierConstructedType, out _))
        {
            throw new UnreachableCodeException();
        }

        OpenApiSchema referenceSchemaForIdentifier = _defaultSchemaGenerator.GenerateSchema(relationshipIdentifierConstructedType, schemaRepository);
        OpenApiSchema fullSchemaForIdentifier = schemaRepository.Schemas[referenceSchemaForIdentifier.Reference.Id];

        fullSchemaForIdentifier.Properties.Remove(JsonApiPropertyName.Meta);

        SetResourceType(fullSchemaForIdentifier, relationship.LeftType, schemaRepository);
        SetResourceId(fullSchemaForIdentifier, relationship.LeftType, schemaRepository);
        SetRelationship(fullSchemaForIdentifier, relationship, schemaRepository);

#if NET6_0
        fullSchemaForIdentifier.ReorderProperties(RelationshipIdentifierPropertyNamesInOrder);
#endif

        schemaRepository.ReplaceSchemaId(relationshipIdentifierConstructedType, schemaId);
        referenceSchemaForIdentifier.Reference.Id = schemaId;

        return referenceSchemaForIdentifier;
    }

    private void SetResourceType(OpenApiSchema fullSchemaForIdentifier, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForIdentifier.Properties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
    }

    private void SetResourceId(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForResourceData.Properties[JsonApiPropertyName.Id] = idSchema;
    }

    private void SetRelationship(OpenApiSchema fullSchemaForIdentifier, RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _relationshipNameSchemaGenerator.GenerateSchema(relationship, schemaRepository);
        fullSchemaForIdentifier.Properties[JsonApiPropertyName.Relationship] = referenceSchema.WrapInExtendedSchema();
    }
}
